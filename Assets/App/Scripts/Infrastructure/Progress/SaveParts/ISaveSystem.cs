using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    public interface ISaveSystem
    {
        public void Register(IProgressRepository repository);
        public void Unregister(IProgressRepository repository);
        public UniTask LoadAllAsync();
        public UniTask LoadAsync<TRepository>() where TRepository : class, IProgressRepository;
        public UniTask LoadAsync(IProgressRepository repository);
        public UniTask LoadAsync(IEnumerable<IProgressRepository> repositories);
        public UniTask SaveDirtyAsync();
        public UniTask SaveAllAsync();
        public UniTask SaveAsync<TRepository>() where TRepository : class, IProgressRepository;
    }
}
