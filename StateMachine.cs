using System.Collections.Generic;
using System.Diagnostics;
using FTStateMachine.Interfaces;
using FTStateMachine.Triggers;

namespace FTStateMachine
{
    public class StateMachine<TStateToken> : IStateMachine<TStateToken>
    {
        private Dictionary<TStateToken, State<TStateToken>> States { get; }
        private TStateToken StartingStateToken { get; }
        private State<TStateToken> CurrentState { get; set; }
        private readonly object _lock = new object();

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

        public void Start()
        {
            GoToStartingState();
        }

        public void Dispatch(object trigger)
        {
            lock(_lock)
            {
                while (true)
                {
                    if (CurrentState == null)
                    {
                        return;
                    }

                    var triggerResult = CurrentState.OnTriggerDispatch(trigger);
                    var transitionedToNewState = GoToState(triggerResult.StateToTransitionTo);
                    if (transitionedToNewState && triggerResult.ForwardTrigger)
                    {
                        continue;
                    }
                    break;
                }
            }
        }

        private bool GoToState(TStateToken stateToken)
        {
            lock (_lock)
            {
                if (CurrentState != null && CurrentState.Token.Equals(stateToken))
                {
                    return false;
                }

                if (States.TryGetValue(stateToken, out State<TStateToken> newState))
                {
                    Dispatch(new StateExitedTrigger());
                    CurrentState = newState;
                    Dispatch(new StateEnteredTrigger());

#if DEBUG
                Debug.WriteLine($" - {typeof(TStateToken).Name}: {CurrentState.Token}");
#endif

                    return true;
                }
                return false;
            }
        }

        public void GoToStartingState()
        {
            GoToState(StartingStateToken);
        }
    }
}
