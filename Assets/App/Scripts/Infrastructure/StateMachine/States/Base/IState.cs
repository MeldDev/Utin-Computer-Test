namespace UtinComputerTest.Infrastructure.StateMachine.States.Base
{
    public interface IState : IExitableState
    {
        public void Enter();
    }
}
