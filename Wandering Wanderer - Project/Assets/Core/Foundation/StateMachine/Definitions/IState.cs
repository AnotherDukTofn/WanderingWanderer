namespace Core.Foundation.StateMachine
{
    public interface IState
    {
        void Enter();
        void Action();
        void FixedAction();
        void Exit();
    }
}