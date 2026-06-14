namespace SportsLibrary.Core
{
    public sealed class PenaltyWinnerTracker : ITimelineListener
    {
        public IContestant? Winner { get; private set; }

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            if (gameEvent.GetEvent() is IPenaltyWinnerEventPayload payload)
                Winner = payload.Winner;
        }
    }
}