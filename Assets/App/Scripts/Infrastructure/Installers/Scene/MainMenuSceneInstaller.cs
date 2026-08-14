using UtinComputerTest.UI.MainMenu;
using UnityEngine;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MainMenuSceneInstaller : MonoInstaller
    {
        [SerializeField] private MenuView _menuView;

        public override void InstallBindings()
        {
            Container.Bind<MenuView>().FromInstance(_menuView).AsSingle();
            Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();
        }
    }
}
