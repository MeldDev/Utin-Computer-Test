using UtinComputerTest.Infrastructure._Services.SceneLoader;
using UtinComputerTest.Infrastructure.StateMachine.States.Base;

namespace UtinComputerTest.Infrastructure.StateMachine.States.Project
{
    public sealed class SceneLoadingState : IStateWithArgument<SceneID>
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ISceneLoader _sceneLoader;

        public SceneLoadingState(IGameStateMachine gameStateMachine, ISceneLoader sceneLoader)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
        }

        public async void Enter(SceneID sceneId)
        {
            _gameStateMachine.ClearSceneStates();
            await _sceneLoader.LoadAsync(sceneId);
        }
    }
}
