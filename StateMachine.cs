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
        private readonly Mutex _mutex = new Mutex();

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
            _mutex.WaitOne();
            while (true)
            {
                if (CurrentState == null)
                {
                    break;
                }

                var triggerResult = await CurrentState.OnTriggerDispatchAsync(trigger);
                var transitionedToNewState = await GoToStateAsync(triggerResult.StateToTransitionTo);
                if (transitionedToNewState && triggerResult.ForwardTrigger)
                {
                    continue;
                }
                break;
            }
            _mutex.ReleaseMutex();
        }

        private async Task<bool> GoToStateAsync(TStateToken stateToken)
        {
            bool result;
            _mutex.WaitOne();
            if (CurrentState != null && CurrentState.Token.Equals(stateToken))
            {
                result = false;
            }
            else if (States.TryGetValue(stateToken, out State<TStateToken> newState))
            {
                await DispatchAsync(new StateExitedTrigger());
                CurrentState = newState;
                await DispatchAsync(new StateEnteredTrigger());
#if DEBUG
                Debug.WriteLine($" - {typeof(TStateToken).Name}: {CurrentState.Token}");
#endif
                result = true;
            }
            else
            {
                result = false;
            }
            _mutex.ReleaseMutex();
            return result;
        }
            
        public async Task GoToStartingStateAsync()
        {
            await GoToStateAsync(StartingStateToken);
        }
    }
}
