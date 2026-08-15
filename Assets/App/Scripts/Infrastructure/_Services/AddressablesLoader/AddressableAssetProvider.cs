using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace UtinComputerTest.Infrastructure.Services.AddressableLoading
{
    public class AddressableAssetProvider : IAddressableAssetProvider
    {
        private readonly IAddressablesLoader _addressablesLoader;
        private readonly Dictionary<string, UnityEngine.Object> _loadedAssetsByUntypedKey = new();
        private readonly Dictionary<string, AsyncOperationHandle> _assetHandlesByUntypedKey = new();
        private readonly Dictionary<string, UnityEngine.Object> _typedAssets = new();
        private readonly Dictionary<string, Component> _loadedComponents = new();
        private readonly Dictionary<string, Task<UnityEngine.Object>> _loadingTasksByUntypedKey = new();
        private readonly Dictionary<string, int> _manualRefCountsByUntypedKey = new();

        public AddressableAssetProvider(IAddressablesLoader addressablesLoader)
        {
            _addressablesLoader = addressablesLoader;
        }

        public async UniTask<T> LoadAsync<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var untypedKey = GetUntypedCacheKey(assetReference);
            var typedKey = GetTypedCacheKey<T>(untypedKey);

            if (_typedAssets.TryGetValue(typedKey, out var typedAsset))
                return typedAsset as T;

            if (_loadedAssetsByUntypedKey.TryGetValue(untypedKey, out var loadedAsset))
                return CacheTypedAsset<T>(loadedAsset, typedKey, untypedKey);

            if (_loadingTasksByUntypedKey.TryGetValue(untypedKey, out var loadingTask))
                return await AwaitAndCacheTypedResult<T>(loadingTask, typedKey, untypedKey);

            var loadTask = LoadAndCacheAsync<T>(assetReference, untypedKey);
            _loadingTasksByUntypedKey[untypedKey] = loadTask;

            try
            {
                return await AwaitAndCacheTypedResult<T>(loadTask, typedKey, untypedKey);
            }
            finally
            {
                _loadingTasksByUntypedKey.Remove(untypedKey);
            }
        }

        public async UniTask<T> AcquireAsync<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var loadedAsset = await LoadAsync<T>(assetReference);
            Retain(assetReference);
            return loadedAsset;
        }

        public async UniTask<T> AcquireComponentAsync<T>(AssetReference assetReference) where T : Component
        {
            var loadedComponent = await LoadComponentAsync<T>(assetReference);
            Retain(assetReference);
            return loadedComponent;
        }

        public async UniTask<T> LoadComponentAsync<T>(AssetReference assetReference) where T : Component
        {
            var cacheKey = GetComponentCacheKey<T>(assetReference);
            if (_loadedComponents.TryGetValue(cacheKey, out var loadedComponent))
                return loadedComponent as T;

            var prefab = await LoadAsync<GameObject>(assetReference);
            var component = ExtractComponent<T>(prefab, cacheKey);
            _loadedComponents[cacheKey] = component;
            return component;
        }

        public T GetLoaded<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var untypedKey = GetUntypedCacheKey(assetReference);
            var typedKey = GetTypedCacheKey<T>(untypedKey);

            if (_typedAssets.TryGetValue(typedKey, out var typedAsset))
                return typedAsset as T;

            if (_loadedAssetsByUntypedKey.TryGetValue(untypedKey, out var loadedAsset))
                return CacheTypedAsset<T>(loadedAsset, typedKey, untypedKey);

            throw new InvalidOperationException(
                $"Asset '{typedKey}' was requested before preload. Load it asynchronously before synchronous access.");
        }

        public T GetLoadedComponent<T>(AssetReference assetReference) where T : Component
        {
            var cacheKey = GetComponentCacheKey<T>(assetReference);
            if (_loadedComponents.TryGetValue(cacheKey, out var loadedComponent))
                return loadedComponent as T;

            var prefab = GetLoaded<GameObject>(assetReference);
            var component = ExtractComponent<T>(prefab, cacheKey);
            _loadedComponents[cacheKey] = component;
            return component;
        }

        public bool IsLoaded(AssetReference assetReference)
        {
            return _loadedAssetsByUntypedKey.ContainsKey(GetUntypedCacheKey(assetReference));
        }

        public void Release(AssetReference assetReference)
        {
            var untypedKey = GetUntypedCacheKey(assetReference);
            if (!_manualRefCountsByUntypedKey.TryGetValue(untypedKey, out var refCount))
            {
                Debug.LogWarning($"[AddressableAssetProvider] Release ignored for '{untypedKey}'. Asset was not acquired manually.");
                return;
            }

            if (refCount > 1)
            {
                _manualRefCountsByUntypedKey[untypedKey] = refCount - 1;
                return;
            }

            _manualRefCountsByUntypedKey.Remove(untypedKey);
            ReleaseInternal(untypedKey);
        }

        private async Task<UnityEngine.Object> LoadAndCacheAsync<T>(AssetReference assetReference, string untypedKey) where T : UnityEngine.Object
        {
            var sw = Stopwatch.StartNew();
            var loadedAssetHandle = await _addressablesLoader.LoadAssetHandleAsync<T>(assetReference);
            _assetHandlesByUntypedKey[untypedKey] = loadedAssetHandle.OperationHandle;
            _loadedAssetsByUntypedKey[untypedKey] = loadedAssetHandle.Asset;

            //Debug.Log($"[AddressableAssetProvider] Lazy-loaded {untypedKey} in {sw.Elapsed.TotalSeconds:F2}s");

            return loadedAssetHandle.Asset;
        }

        private async Task<T> AwaitAndCacheTypedResult<T>(Task<UnityEngine.Object> loadingTask, string typedKey, string untypedKey) where T : UnityEngine.Object
        {
            var loadedAsset = await loadingTask;
            return CacheTypedAsset<T>(loadedAsset, typedKey, untypedKey);
        }

        private T CacheTypedAsset<T>(UnityEngine.Object loadedAsset, string typedKey, string untypedKey) where T : UnityEngine.Object
        {
            if (loadedAsset is T typedAsset)
            {
                _typedAssets[typedKey] = typedAsset;
                return typedAsset;
            }

            throw new InvalidOperationException(
                $"Asset '{untypedKey}' was loaded as incompatible type. Requested '{typeof(T).Name}', actual '{loadedAsset?.GetType().Name ?? "null"}'.");
        }

        private void Retain(AssetReference assetReference)
        {
            var untypedKey = GetUntypedCacheKey(assetReference);
            _manualRefCountsByUntypedKey.TryGetValue(untypedKey, out var refCount);
            _manualRefCountsByUntypedKey[untypedKey] = refCount + 1;
        }

        private string GetTypedCacheKey<T>(string untypedKey) where T : UnityEngine.Object
        {
            return $"{untypedKey}::{typeof(T).FullName}";
        }

        private string GetUntypedCacheKey(AssetReference assetReference)
        {
            if (assetReference == null || assetReference.RuntimeKeyIsValid() == false)
                throw new InvalidOperationException("Addressable reference is null or invalid.");

            var runtimeKey = assetReference.RuntimeKey.ToString();
            var subObjectName = assetReference.SubObjectName;

            return string.IsNullOrEmpty(subObjectName)
                ? runtimeKey
                : $"{runtimeKey}[{subObjectName}]";
        }

        private string GetComponentCacheKey<T>(AssetReference assetReference) where T : Component
        {
            return $"{GetUntypedCacheKey(assetReference)}::{typeof(T).FullName}";
        }

        private void ReleaseInternal(string untypedKey)
        {
            if (_assetHandlesByUntypedKey.Remove(untypedKey, out var handle))
                _addressablesLoader.Release(handle);

            _loadedAssetsByUntypedKey.Remove(untypedKey);
            RemoveCachedEntriesByPrefix(_typedAssets, untypedKey);
            RemoveCachedEntriesByPrefix(_loadedComponents, untypedKey);
        }

        private static void RemoveCachedEntriesByPrefix<TValue>(Dictionary<string, TValue> dictionary, string untypedKey)
        {
            var keysToRemove = new List<string>();

            foreach (var key in dictionary.Keys)
            {
                if (key.StartsWith(untypedKey + "::", StringComparison.Ordinal))
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
            {
                dictionary.Remove(key);
            }
        }

        private T ExtractComponent<T>(GameObject prefab, string cacheKey) where T : Component
        {
            if (prefab == null)
                throw new InvalidOperationException($"Prefab '{cacheKey}' is null.");

            var component = prefab.GetComponent<T>();
            if (component != null)
                return component;

            throw new InvalidOperationException(
                $"Prefab '{cacheKey}' does not contain required root component '{typeof(T).Name}'.");
        }
    }
}
