using UtinComputerTest.Infrastructure.Bootstrappers;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class MapSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<MapBootstrapper>().AsSingle();
        }
    }
}
