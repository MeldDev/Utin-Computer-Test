using UtinComputerTest.Infrastructure.Bootstrappers;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class BootstrapSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<AppBootstrapper>().AsSingle();
        }
    }
}
