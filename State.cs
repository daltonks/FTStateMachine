using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FTStateMachine.Interfaces;

namespace FTStateMachine
{
    public class State<TToken> : IState<TToken>
    {
        public delegate Task<TriggerActionResult<TToken>> OnTriggerDelegate(object trigger);

        public TToken Token { get; }

        private Dictionary<Type, List<OnTriggerDelegate>> OnTriggerMap { get; }

        public State(TToken token)
        {
            Token = token;
            OnTriggerMap = new Dictionary<Type, List<OnTriggerDelegate>>();
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
            return OnAsync<TTrigger>(
                predicate,
                (trigger) =>
                {
                    onTrigger?.Invoke(trigger);
                    return Task.FromResult(Token);
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
            return OnAsync(
                predicate,
                new Func<TTrigger, Task<TToken>>(trigger => Task.FromResult(onTrigger.Invoke(trigger))),
                forwardTrigger
            );
        }

        public IState<TToken> OnAsync<TTrigger>(Func<Task<TToken>> onTrigger, bool forwardTrigger = true)
        {
            return OnAsync<TToken>(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> OnAsync<TTrigger>(Func<bool> predicate, Func<Task<TToken>> onTrigger, bool forwardTrigger = true)
        {
            return OnAsync<TToken>(predicate, (trigger) => onTrigger.Invoke(), forwardTrigger);
        }

        public IState<TToken> OnAsync<TTrigger>(Func<TTrigger, Task<TToken>> onTrigger, bool forwardTrigger = true)
        {
            return OnAsync(null, onTrigger, forwardTrigger);
        }

        public IState<TToken> OnAsync<TTrigger>(Func<bool> predicate, Func<TTrigger, Task<TToken>> onTrigger, bool forwardTrigger = true)
        {
            List<OnTriggerDelegate> triggerFuncs;

            if(!OnTriggerMap.TryGetValue(typeof(TTrigger), out triggerFuncs))
            {
                OnTriggerMap[typeof(TTrigger)] = triggerFuncs = new List<OnTriggerDelegate>();
            }

            triggerFuncs.Add(async (o) => {
                if (predicate?.Invoke() ?? true)
                {
                    var stateToTransitionTo = await onTrigger.Invoke((TTrigger) o);
                    return new TriggerActionResult<TToken>(stateToTransitionTo, forwardTrigger);
                }
                else
                {
                    return new TriggerActionResult<TToken>(Token, false);
                }
            });

            return this;
        }

        public async Task<TriggerActionResult<TToken>> OnTriggerDispatchAsync(object trigger)
        {
            if (OnTriggerMap.TryGetValue(trigger.GetType(), out List<OnTriggerDelegate> triggerFuncs))
            {
                var results = new TriggerActionResult<TToken>[triggerFuncs.Count];
                for (var i = 0; i < triggerFuncs.Count; i++)
                {
                    results[i] = await triggerFuncs[i].Invoke(trigger);
                }

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
