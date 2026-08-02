using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FTStateMachine.Interfaces;
using FTStateMachine.Triggers;

namespace FTStateMachine
{
    public class StateMachine<TStateToken> : IStateMachine<TStateToken>
    {
        private Dictionary<TStateToken, State<TStateToken>> States { get; }
        private TStateToken StartingStateToken { get; }
        private State<TStateToken> CurrentState { get; set; }

        /// <summary>
        /// Serializes dispatches. Held only by the public entry points; the
        /// Core methods below assume it is already held, since transitioning
        /// dispatches the enter/exit triggers and would otherwise re-enter.
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public StateMachine(TStateToken startingStateToken)
        {
            StartingStateToken = startingStateToken;
            States = new Dictionary<TStateToken, State<TStateToken>>();
        }

        public IState<TStateToken> Configure(TStateToken stateToken)
        {
            State<TStateToken> state;

            if (States.TryGetValue(stateToken, out state))
            {
                return state;
            }

            state = new State<TStateToken>(stateToken);
            States[stateToken] = state;
            return state;
        }

        public async Task StartAsync()
        {
            await GoToStartingStateAsync();
        }

        public async Task DispatchAsync(object trigger)
        {
            await _semaphore.WaitAsync();
            try
            {
                await DispatchCoreAsync(trigger);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task GoToStartingStateAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await GoToStateCoreAsync(StartingStateToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task DispatchCoreAsync(object trigger)
        {
            while (true)
            {
                if (CurrentState == null)
                {
                    break;
                }

                var triggerResult = await CurrentState.OnTriggerDispatchAsync(trigger);
                var transitionedToNewState = await GoToStateCoreAsync(triggerResult.StateToTransitionTo);
                if (transitionedToNewState && triggerResult.ForwardTrigger)
                {
                    continue;
                }
                break;
            }
        }

        private async Task<bool> GoToStateCoreAsync(TStateToken stateToken)
        {
            if (CurrentState != null && CurrentState.Token.Equals(stateToken))
            {
                return false;
            }

            if (!States.TryGetValue(stateToken, out State<TStateToken> newState))
            {
                return false;
            }

            await DispatchCoreAsync(new StateExitedTrigger());
            CurrentState = newState;
            await DispatchCoreAsync(new StateEnteredTrigger());
#if DEBUG
            Debug.WriteLine($" - {typeof(TStateToken).Name}: {CurrentState.Token}");
#endif
            return true;
        }
    }
}
