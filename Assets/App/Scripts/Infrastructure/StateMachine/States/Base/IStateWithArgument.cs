namespace UtinComputerTest.Infrastructure.StateMachine.States.Base {
    public interface IStateWithArgument<in TArgs> : IExitableState {
        void Enter(TArgs combatResultInfo);
    }
}