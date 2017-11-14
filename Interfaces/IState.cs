using System;
using System.Threading.Tasks;

namespace FTStateMachine.Interfaces
{
    public interface IState<TToken>
    {
        IState<TToken> On<TTrigger>(Action onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Action onTrigger, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(Action<TTrigger> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Action<TTrigger> onTrigger, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(TToken outboundState, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, TToken outboundState, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(Func<TToken> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TToken> onTrigger, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true);

        IState<TToken> OnAsync<TTrigger>(Func<Task<TToken>> onTrigger, bool forwardTrigger = true);
        IState<TToken> OnAsync<TTrigger>(Func<bool> predicate, Func<Task<TToken>> onTrigger, bool forwardTrigger = true);

        IState<TToken> OnAsync<TTrigger>(Func<TTrigger, Task<TToken>> onTrigger, bool forwardTrigger = true);
        IState<TToken> OnAsync<TTrigger>(Func<bool> predicate, Func<TTrigger, Task<TToken>> onTrigger, bool forwardTrigger = true);
    }
}
