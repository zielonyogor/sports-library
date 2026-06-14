namespace SportsLibrary.Core
{
    public sealed class MatchStateTracker : ITimelineListener
    {
        public MatchState? CurrentState { get; private set; }

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            if (gameEvent.GetEvent() is IMatchStateEventPayload payload)
                CurrentState = payload.ResultingState;
        }
    }
}