using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    public class SaveSystem : ISaveSystem
    {
        private readonly ISaveBackend _backend;
        private readonly Dictionary<string, IProgressRepository> _repositoriesByPartId = new();
        private readonly Dictionary<Type, IProgressRepository> _repositoriesByType = new();

        public SaveSystem(ISaveBackend backend)
        {
            _backend = backend;
        }

        public void Register(IProgressRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (!_repositoriesByPartId.TryAdd(repository.PartId, repository))
            {
                throw new InvalidOperationException($"Repository with part id '{repository.PartId}' is already registered.");
            }

            if (_repositoriesByType.TryAdd(repository.GetType(), repository)) return;

            _repositoriesByPartId.Remove(repository.PartId);
            throw new InvalidOperationException($"Repository type '{repository.GetType().Name}' is already registered.");
        }

        public void Unregister(IProgressRepository repository)
        {
            if (repository == null)
            {
                return;
            }

            if (_repositoriesByPartId.TryGetValue(repository.PartId, out var registeredRepository) &&
                ReferenceEquals(registeredRepository, repository))
            {
                _repositoriesByPartId.Remove(repository.PartId);
            }

            var repositoryType = repository.GetType();
            if (_repositoriesByType.TryGetValue(repositoryType, out registeredRepository) &&
                ReferenceEquals(registeredRepository, repository))
            {
                _repositoriesByType.Remove(repositoryType);
            }
        }

        public async UniTask LoadAllAsync()
        {
            foreach (var repository in GetRegisteredRepositories())
            {
                await repository.LoadAsync();
            }
        }

        public async UniTask LoadAsync<TRepository>() where TRepository : class, IProgressRepository
        {
            await GetRepository<TRepository>().LoadAsync();
        }

        public async UniTask LoadAsync(IProgressRepository repository)
        {
            var registeredRepository = EnsureRegistered(repository);
            await registeredRepository.LoadAsync();
        }

        public async UniTask LoadAsync(IEnumerable<IProgressRepository> repositories)
        {
            if (repositories == null)
            {
                return;
            }

            foreach (var repository in repositories)
            {
                await LoadAsync(repository);
            }
        }

        public async UniTask SaveDirtyAsync()
        {
            var repositories = GetRegisteredRepositories()
                .Where(repository => repository.AutoSaveEnabled && repository.IsDirty)
                .ToArray();

            if (repositories.Length == 0)
            {
                return;
            }

            var hasSavedAnyRepository = false;
            foreach (var repository in repositories)
            {
                var wasDirty = repository.IsDirty;
                await repository.SaveAsync();
                hasSavedAnyRepository |= wasDirty;
            }

            if (hasSavedAnyRepository)
            {
                await _backend.FlushAsync();
            }
        }

        public async UniTask SaveAllAsync()
        {
            var repositories = GetRegisteredRepositories();
            if (repositories.Count == 0)
            {
                return;
            }

            foreach (var repository in repositories)
            {
                await repository.SaveAsync();
            }

            await _backend.FlushAsync();
        }

        public async UniTask SaveAsync<TRepository>() where TRepository : class, IProgressRepository
        {
            var repository = GetRepository<TRepository>();
            await repository.SaveAsync();
            await _backend.FlushAsync();
        }

        private TRepository GetRepository<TRepository>() where TRepository : class, IProgressRepository
        {
            if (_repositoriesByType.TryGetValue(typeof(TRepository), out var repository))
            {
                return repository as TRepository;
            }

            throw new InvalidOperationException($"Repository '{typeof(TRepository).Name}' is not registered.");
        }

        private IReadOnlyList<IProgressRepository> GetRegisteredRepositories()
        {
            return _repositoriesByPartId.Values
                .OrderBy(repository => repository.PartId, StringComparer.Ordinal)
                .ToArray();
        }

        private IProgressRepository EnsureRegistered(IProgressRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (!_repositoriesByPartId.TryGetValue(repository.PartId, out var registeredRepository) ||
                !ReferenceEquals(registeredRepository, repository))
            {
                throw new InvalidOperationException(
                    $"Repository '{repository.GetType().Name}' with part id '{repository.PartId}' is not registered.");
            }

            return registeredRepository;
        }
    }
}
