using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace UtinComputerTest.Infrastructure._Services.AddressablesLoader
{
    public interface IAddressablesLoader
    {
        UniTask<AddressableAssetLoadHandle<T>> LoadAssetHandleAsync<T>(AssetReference assetReference) where T : UnityEngine.Object;
        UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : UnityEngine.Object;
        UniTask<GameObject> LoadGameObjectAsync(AssetReferenceGameObject assetReference);
        UniTask<Scene> LoadSceneAsync(AssetReference sceneReference, IProgress<float> progress = null);
        UniTask UnloadSceneAsync(Scene scene);
        void Release(AsyncOperationHandle handle);
    }
}
