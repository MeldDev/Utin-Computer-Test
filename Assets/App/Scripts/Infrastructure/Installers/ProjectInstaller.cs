using UtinComputerTest.Infrastructure.Services.AddressableLoading;
using UtinComputerTest.Infrastructure.Services.SceneLoading;
using UtinComputerTest.Infrastructure.StateMachine;
using UtinComputerTest.Infrastructure.StateMachine.States.Project;
using UtinComputerTest.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private AssetReferenceT<ScenesAddresses> _scenesAddresses;

        public override void InstallBindings()
        {
            Container.Bind<IAddressablesLoader>().To<AddressablesLoader>().AsSingle();
            Container.Bind<IAddressableAssetProvider>().To<AddressableAssetProvider>().AsSingle();
            Container.Bind<ISceneContentReadiness>().To<SceneContentReadiness>().AsSingle();
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle().WithArguments(_scenesAddresses);
            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();
            Container.Bind<SceneLoadingState>().AsSingle();
            Container.Bind<ProjectPrefabsPreloadState>().AsSingle();
            Container.Bind<ProjectInitializationState>().AsSingle();
        }
    }
}
