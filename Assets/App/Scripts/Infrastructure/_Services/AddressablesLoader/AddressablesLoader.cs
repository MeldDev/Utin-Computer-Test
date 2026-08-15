using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UtinComputerTest.Infrastructure.Services.AddressableLoading
{
    public class AddressablesLoader : IAddressablesLoader
    {
        private readonly Dictionary<Scene, SceneInstance> _loadedScenes = new();

        public async UniTask<AddressableAssetLoadHandle<T>> LoadAssetHandleAsync<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            ValidateAssetReference<T>(assetReference);

            var handle = Addressables.LoadAssetAsync<T>(assetReference);
            await handle.Task;

            EnsureAssetLoadedSuccessfully(assetReference, handle);
            return new AddressableAssetLoadHandle<T>(handle.Result, handle);
        }

        public async UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            var loadedAsset = await LoadAssetHandleAsync<T>(assetReference);
            return loadedAsset.Asset;
        }

        public async UniTask<GameObject> LoadGameObjectAsync(AssetReferenceGameObject assetReference)
        {
            return await LoadAssetAsync<GameObject>(assetReference);
        }

        public async UniTask<Scene> LoadSceneAsync(AssetReference sceneReference, IProgress<float> progress = null)
        {
            if (sceneReference.RuntimeKeyIsValid() == false)
            {
                Debug.LogError("Unable to load Scene. AssetReference is null");
                return default;
            }

            var loadHandle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive, false);

            while (!loadHandle.IsDone)
            {
                progress?.Report(loadHandle.PercentComplete * 0.9f);
                await UniTask.Yield();
            }

            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Unable to load Scene. Key='{sceneReference.RuntimeKey}'. Status='{loadHandle.Status}'. " +
                               $"Exception: {loadHandle.OperationException}");

                if (loadHandle.IsValid())
                    Addressables.Release(loadHandle);

                return default;
            }

            var sceneInstance = loadHandle.Result;

            var activateOperation = sceneInstance.ActivateAsync();
            while (!activateOperation.isDone)
            {
                float p = 0.9f + 0.1f * activateOperation.progress;
                progress?.Report(p);
                await UniTask.Yield();
            }

            progress?.Report(1f);

            _loadedScenes.Add(sceneInstance.Scene, sceneInstance);
            return sceneInstance.Scene;
        }

        public async UniTask UnloadSceneAsync(Scene scene)
        {
            if (!_loadedScenes.Remove(scene, out var sceneInstance))
                return;

            if (scene.IsValid())
            {
                var unloadHandle = Addressables.UnloadSceneAsync(sceneInstance);
                await unloadHandle.Task;
            }
        }

        public void Release(AsyncOperationHandle handle)
        {
            if (handle.IsValid() == false)
                return;

            Addressables.Release(handle);
        }

        private static void ValidateAssetReference<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            if (assetReference != null && assetReference.RuntimeKeyIsValid())
                return;

            throw new InvalidOperationException(
                $"Unable to load asset of type '{typeof(T).Name}'. Addressable reference is null or invalid.");
        }

        private static void EnsureAssetLoadedSuccessfully<T>(AssetReference assetReference, AsyncOperationHandle<T> handle) where T : UnityEngine.Object
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                return;

            var operationException = handle.OperationException;
            var message =
                $"Unable to load asset of type '{typeof(T).Name}'. Key='{assetReference.RuntimeKey}'. Status='{handle.Status}'. Exception: {operationException}";

            Debug.LogError(message);

            if (handle.IsValid())
                Addressables.Release(handle);

            throw new InvalidOperationException(message, operationException);
        }
    }
}
