using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UtinComputerTest.Infrastructure.Services.AddressableLoading
{
    // Load*    — shared доступ без владения, актив живёт до конца сессии/сцены.
    // Acquire* — caller берёт владение, обязан вызвать Release.
    public interface IAddressableAssetProvider
    {
        UniTask<T> LoadAsync<T>(AssetReference assetReference) where T : Object;
        UniTask<T> AcquireAsync<T>(AssetReference assetReference) where T : Object;
        UniTask<T> AcquireComponentAsync<T>(AssetReference assetReference) where T : Component;
        UniTask<T> LoadComponentAsync<T>(AssetReference assetReference) where T : Component;
        T GetLoaded<T>(AssetReference assetReference) where T : Object;
        T GetLoadedComponent<T>(AssetReference assetReference) where T : Component;
        bool IsLoaded(AssetReference assetReference);
        void Release(AssetReference assetReference);
    }
}
