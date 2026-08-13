using UtinComputerTest.Infrastructure.StateMachine.States.Base;

namespace UtinComputerTest.Infrastructure.StateMachine
{
    public interface IGameStateMachine : IStateMachine
    {
        public void AddSceneState<TState>(TState state) where TState : IExitableState;
        public void ClearSceneStates();
    }
}
