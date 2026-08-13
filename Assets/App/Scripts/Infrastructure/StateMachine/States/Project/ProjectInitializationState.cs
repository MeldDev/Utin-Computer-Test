using UtinComputerTest.Infrastructure._Services.SceneLoader;
using UtinComputerTest.Infrastructure.StateMachine.States.Base;

namespace UtinComputerTest.Infrastructure.StateMachine.States.Project
{
    public sealed class ProjectInitializationState : IState
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SceneLoadingState _sceneLoadingState;

        public ProjectInitializationState(
            IGameStateMachine gameStateMachine,
            SceneLoadingState sceneLoadingState)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoadingState = sceneLoadingState;
        }

        public void Enter()
        {
            _gameStateMachine.AddSceneState(_sceneLoadingState);
            _gameStateMachine.EnterState<SceneLoadingState, SceneID>(SceneID.MainMenu);
        }
    }
}
