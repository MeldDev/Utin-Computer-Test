using UtinComputerTest.Infrastructure.StateMachine;
using UtinComputerTest.Infrastructure.StateMachine.States.Project;
using Zenject;

namespace UtinComputerTest.Infrastructure.Bootstrappers
{
    public sealed class AppBootstrapper : IInitializable
    {
        private readonly IGameStateMachine _gameStateMachine;

        public AppBootstrapper(
            IGameStateMachine gameStateMachine,
            ProjectPrefabsPreloadState projectPrefabsPreloadState,
            ProjectInitializationState projectInitializationState)
        {
            _gameStateMachine = gameStateMachine;
            _gameStateMachine.AddState(projectPrefabsPreloadState);
            _gameStateMachine.AddState(projectInitializationState);
        }

        public void Initialize()
        {
            _gameStateMachine.EnterState<ProjectPrefabsPreloadState>();
        }
    }
}
