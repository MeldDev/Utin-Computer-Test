using System;
using Cysharp.Threading.Tasks;
using UtinComputerTest.Infrastructure.Progress.SaveLoad.Modules.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    // Репозиторий регистрируется в SaveSystem из конструктора, поэтому перед загрузкой
    // нужно резолвить конкретную load-group, к которой относится репозиторий.
    public abstract class ProgressRepository<TState> : IProgressRepository, IDisposable
        where TState : class
    {
        private readonly ISaveSystem _saveSystem;
        private readonly ISerializer _serializer;
        private readonly ISaveBackend _backend;

        private string _lastPersistedPayload;
        private bool _isSubscribed;

        protected ProgressRepository(
            ISaveSystem saveSystem,
            ISerializer serializer,
            ISaveBackend backend)
        {
            _saveSystem = saveSystem;
            _serializer = serializer;
            _backend = backend;
            _saveSystem.Register(this);
        }

        public abstract string PartId { get; }
        public virtual int Version => 1;
        public virtual bool AutoSaveEnabled => true;
        public bool IsDirty { get; private set; }
        public bool IsLoaded { get; private set; }

        public void Dispose()
        {
            if (_isSubscribed)
            {
                UnsubscribeFromChanges();
            }

            _saveSystem.Unregister(this);
        }

        public async UniTask LoadAsync()
        {
            EnsureSubscribed();

            if (IsLoaded)
            {
                return;
            }

            if (await _backend.ExistsAsync(PartId))
            {
                var payload = await _backend.ReadAsync(PartId);
                if (string.IsNullOrWhiteSpace(payload))
                {
                    LoadDefault();
                    return;
                }

                var envelope = await _serializer.DeserializeAsync<RepositoryEnvelope<TState>>(payload)
                               ?? throw new InvalidOperationException($"{GetType().Name} deserialized null repository envelope.");
                if (envelope.Version != Version)
                {
                    throw new NotSupportedException(
                        $"{GetType().Name} does not support version '{envelope.Version}'. Expected '{Version}'.");
                }

                var state = envelope.State
                            ?? throw new InvalidOperationException($"{GetType().Name} deserialized null state.");

                RestoreState(state);
                _lastPersistedPayload = payload;
                IsLoaded = true;
                IsDirty = false;
                return;
            }

            LoadDefault();
        }

        public async UniTask SaveAsync()
        {
            EnsureSubscribed();

            if (!IsLoaded)
            {
                await LoadAsync();
            }

            var payload = await CreatePayloadAsync();

            if (AreEquivalentPayloads(payload, _lastPersistedPayload))
            {
                IsDirty = false;
                return;
            }

            await _backend.WriteAsync(PartId, payload);
            _lastPersistedPayload = payload;
            IsDirty = false;
        }

        protected void MarkDirty()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException($"{GetType().Name} received a change before load completed.");
            }

            IsDirty = true;
        }

        protected virtual TState CreateDefaultState()
        {
            throw new NotSupportedException(
                $"{GetType().Name} must override {nameof(CreateDefaultState)}.");
        }

        protected abstract TState CaptureState();
        protected abstract void RestoreState(TState state);
        protected abstract void SubscribeToChanges();
        protected abstract void UnsubscribeFromChanges();

        private void LoadDefault()
        {
            var defaultState = CreateDefaultState()
                ?? throw new InvalidOperationException($"{GetType().Name} returned null default state.");

            RestoreState(defaultState);
            _lastPersistedPayload = null;
            IsLoaded = true;
            IsDirty = true;
        }

        private async UniTask<string> CreatePayloadAsync()
        {
            var state = CaptureState()
                ?? throw new InvalidOperationException($"{GetType().Name} returned null captured state.");
            var envelope = new RepositoryEnvelope<TState>
            {
                Version = Version,
                State = state
            };

            return await _serializer.SerializeAsync(envelope);
        }

        private static bool AreEquivalentPayloads(string left, string right)
        {
            if (string.Equals(left, right, StringComparison.Ordinal))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            try
            {
                return JToken.DeepEquals(JToken.Parse(left), JToken.Parse(right));
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private void EnsureSubscribed()
        {
            if (_isSubscribed)
            {
                return;
            }

            SubscribeToChanges();
            _isSubscribed = true;
        }

        [Serializable]
        private sealed class RepositoryEnvelope<TEnvelopeState>
        {
            [JsonProperty(Order = 0)]
            public int Version;

            [JsonProperty(Order = 1)]
            public TEnvelopeState State;
        }
    }
}
