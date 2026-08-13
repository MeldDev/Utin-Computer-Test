using UtinComputerTest.Infrastructure.StateMachine.States.Base;

namespace UtinComputerTest.Infrastructure.StateMachine
{
    public interface IStateMachine
    {
        public void EnterState<TState>() where TState : IState;
        public void EnterState<TState, TArgument>(TArgument argument) where TState : IStateWithArgument<TArgument>;
        public void AddState<TState>(TState state) where TState : IExitableState;
    }
}
