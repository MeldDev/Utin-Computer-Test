using UtinComputerTest.Infrastructure.Bootstrappers;
using UtinComputerTest.Gameplay.Configs;
using UtinComputerTest.Gameplay.Runtime;
using UtinComputerTest.Gameplay.Views;
using UtinComputerTest.Infrastructure.Services.AddressableLoading;
using UtinComputerTest.UI.Providers;
using UtinComputerTest.UI.Windows;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MapSceneInstaller : MonoInstaller
    {
        [SerializeField] private AssetReferenceT<AddressableConfigsCatalog> _configsCatalog;
        [SerializeField] private GameplayDebugView _gameplayDebugView;
        [SerializeField] private GameplayCameraView _gameplayCameraView;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private UILayers _uiLayers;

        public override void InstallBindings()
        {
            Container.BindInstance(new GameplayAddressables(_configsCatalog)).AsSingle();
            Container.Bind<IUIProvider>().To<UIProvider>().AsSingle().WithArguments(_canvas, _uiLayers);
            Container.Bind<IWindowFactory>().To<MapWindowFactory>().AsSingle();
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
            Container.Bind<IGameplayFlowService>().To<GameplayFlowService>().AsSingle();
            Container.BindInterfacesTo<WinWindowPresenter>().AsSingle();
            Container.BindInterfacesTo<LoseWindowPresenter>().AsSingle();
            Container.Bind<PlayerPathService>().AsSingle();
            if (_gameplayDebugView != null)
            {
                Container.BindInstance(_gameplayDebugView).AsSingle();
            }
            Container.BindInstance(_gameplayCameraView).AsSingle();
            Container.BindInterfacesAndSelfTo<MapBootstrapper>().AsSingle();
        }
    }
}
