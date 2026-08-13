using UtinComputerTest.UI.MainMenu;
using UtinComputerTest.Infrastructure._Services.SceneLoader;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MainMenuSceneInstaller : MonoInstaller
    {
        [SerializeField] private MenuView _menuView;

        public override void InstallBindings()
        {
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
            Container.Bind<MenuView>().FromInstance(_menuView).AsSingle();
            Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();
        }
    }
}
