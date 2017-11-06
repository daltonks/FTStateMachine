namespace FTStateMachine.Interfaces
{
    public interface IStateMachine
    {
        void Start();
        void Dispatch(object trigger);
        void GoToStartingState();
    }

    public interface IStateMachine<TStateToken> : IStateMachine
    {
        IState<TStateToken> Configure(TStateToken stateToken);
    }
}
