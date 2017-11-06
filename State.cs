using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FTStateMachine.Interfaces;

namespace FTStateMachine
{
    public class State<TToken> : IState<TToken>
    {
        public TToken Token { get; }

        private Dictionary<Type, List<Func<object, TriggerActionResult<TToken>>>> Triggers { get; }

        public State(TToken token)
        {
            Token = token;
            Triggers = new Dictionary<Type, List<Func<object, TriggerActionResult<TToken>>>>();
        }

        public IState<TToken> On<TTrigger>(Action onTrigger, bool forwardTrigger = true)
        {
            return On<TTrigger>(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<bool> predicate, Action onTrigger, bool forwardTrigger = true)
        {
            return On<TTrigger>(predicate, (trigger) => onTrigger.Invoke(), forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Action<TTrigger> onTrigger, bool forwardTrigger = true)
        {
            return On(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<bool> predicate, Action<TTrigger> onTrigger, bool forwardTrigger = true)
        {
            return On<TTrigger>(
                predicate,
                (trigger) =>
                {
                    onTrigger?.Invoke(trigger);
                    return Token;
                },
                forwardTrigger
            );
        }

        public IState<TToken> On<TTrigger>(TToken outboundState, bool forwardTrigger = true)
        {
            return On<TTrigger>(null, outboundState, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<bool> predicate, TToken outboundState, bool forwardTrigger = true)
        {
            return On<TTrigger>(predicate, () => outboundState, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<TToken> onTrigger, bool forwardTrigger = true)
        {
            return On<TTrigger>(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TToken> onTrigger, bool forwardTrigger = true)
        {
            return On<TTrigger>(predicate, (trigger) => onTrigger.Invoke(), forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true)
        {
            return On(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> On<TTrigger>(Func<bool> predicate, Func<TTrigger, TToken> onTrigger, bool forwardTrigger = true)
        {
            List<Func<object, TriggerActionResult<TToken>>> triggerFuncs;

            if(!Triggers.TryGetValue(typeof(TTrigger), out triggerFuncs))
            {
                Triggers[typeof(TTrigger)] = triggerFuncs = new List<Func<object, TriggerActionResult<TToken>>>();
            }

            triggerFuncs.Add((o) =>
            {
                if (predicate?.Invoke() ?? true)
                {
                    var stateToTransitionTo = onTrigger.Invoke((TTrigger) o);
                    return new TriggerActionResult<TToken>(stateToTransitionTo, forwardTrigger);
                }
                else
                {
                    return new TriggerActionResult<TToken>(Token, false);
                }
            });

            return this;
        }

        public TriggerActionResult<TToken> OnTriggerDispatch(object trigger)
        {
            if (Triggers.TryGetValue(trigger.GetType(), out List<Func<object, TriggerActionResult<TToken>>> triggerFuncs))
            {
                var results = triggerFuncs.Select(f => f.Invoke(trigger)).ToArray();
                var resultsWithStateChanges = results.Where(r => !r.StateToTransitionTo.Equals(Token)).ToArray();
                Debug.Assert(resultsWithStateChanges.Length < 2, "Multiple trigger results want state changes!");
                if (resultsWithStateChanges.Any())
                {
                    return resultsWithStateChanges.First();
                }
            }
            return new TriggerActionResult<TToken>(Token, false);
        }
        
        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is State<TToken> otherState)
            {
                return Token.Equals(otherState.Token);
            }
            return false;
        }
    }
}
