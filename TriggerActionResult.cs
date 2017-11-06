namespace FTStateMachine
{
    public struct TriggerActionResult<TToken>
    {
        public TToken StateToTransitionTo { get; }
        public bool ForwardTrigger { get; }

        public TriggerActionResult(TToken stateToTransitionTo, bool forwardTrigger)
        {
            StateToTransitionTo = stateToTransitionTo;
            ForwardTrigger = forwardTrigger;
        }
    }
}
