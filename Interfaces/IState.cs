﻿using System;

namespace FTStateMachine.Interfaces
{
    public interface IState<TToken>
    {
        TToken Token { get; }

        IState<TToken> On<TTrigger>(Action<TTrigger> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Action<TTrigger> onTrigger, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(TToken outboundState, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, TToken outboundState, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(Func<TToken> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TToken> onTrigger, bool forwardTrigger = true);

        IState<TToken> On<TTrigger>(Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true);
        IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true);

        TriggerActionResult<TToken> OnTriggerDispatch(object trigger);
    }
}
