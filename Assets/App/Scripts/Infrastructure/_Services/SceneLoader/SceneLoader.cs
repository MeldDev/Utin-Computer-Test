using Cysharp.Threading.Tasks;
using UtinComputerTest.Infrastructure._Services.AddressablesLoader;
using UtinComputerTest.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UtinComputerTest.Infrastructure._Services.SceneLoader
{
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly IAddressablesLoader _addressablesLoader;
        private readonly IAddressableAssetProvider _assetProvider;
        private readonly AssetReferenceT<ScenesAddresses> _scenesAddressesReference;

        public SceneLoader(
            IAddressablesLoader addressablesLoader,
            IAddressableAssetProvider assetProvider,
            AssetReferenceT<ScenesAddresses> scenesAddressesReference)
        {
            _addressablesLoader = addressablesLoader;
            _assetProvider = assetProvider;
            _scenesAddressesReference = scenesAddressesReference;
        }

        public async UniTask LoadAsync(SceneID sceneId)
        {
            var previousScene = SceneManager.GetActiveScene();
            var scenesAddresses = await _assetProvider.LoadAsync<ScenesAddresses>(_scenesAddressesReference);
            var sceneReference = GetSceneReference(sceneId, scenesAddresses);
            var loadingScene = await _addressablesLoader.LoadSceneAsync(scenesAddresses.Loading);
            var loadedScene = await _addressablesLoader.LoadSceneAsync(sceneReference);

            SceneManager.SetActiveScene(loadedScene);
            await _addressablesLoader.UnloadSceneAsync(loadingScene);

            if (previousScene.IsValid())
            {
                await _addressablesLoader.UnloadSceneAsync(previousScene);
                if (previousScene.isLoaded)
                {
                    await SceneManager.UnloadSceneAsync(previousScene);
                }
            }
        }

        private static AssetReference GetSceneReference(SceneID sceneId, ScenesAddresses scenesAddresses)
        {
            return sceneId switch
            {
                SceneID.MainMenu => scenesAddresses.MainMenu,
                SceneID.Map => scenesAddresses.Map,
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneId), sceneId, null)
            };
        }
    }
}
