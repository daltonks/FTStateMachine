using System.Threading.Tasks;

namespace FTStateMachine.Interfaces
{
    public interface IStateMachine
    {
        Task StartAsync();
        Task DispatchAsync(object trigger);
        Task GoToStartingStateAsync();
    }

    public interface IStateMachine<TStateToken> : IStateMachine
    {
        IState<TStateToken> Configure(TStateToken stateToken);
    }
}
