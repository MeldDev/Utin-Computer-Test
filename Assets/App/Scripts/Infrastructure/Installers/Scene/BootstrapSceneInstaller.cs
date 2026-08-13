using UtinComputerTest.Infrastructure.Bootstrappers;
using UtinComputerTest.Infrastructure._Services.SceneLoader;
using UtinComputerTest.Infrastructure.StateMachine;
using UtinComputerTest.Infrastructure.StateMachine.States.Project;
using Zenject;

namespace UtinComputerTest.Infrastructure.Installers.Scene
{
    public sealed class BootstrapSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
            Container.Bind<SceneLoadingState>().AsSingle();
            Container.Bind<ProjectPrefabsPreloadState>().AsSingle();
            Container.Bind<ProjectInitializationState>().AsSingle();
            Container.BindInterfacesTo<AppBootstrapper>().AsSingle();
        }
    }
}
