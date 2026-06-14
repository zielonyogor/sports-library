namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Scheduled.
    /// </summary>
    public class ScheduledMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }
        public DateTime ScheduledDate { get; }

        public ScheduledMatchStateEventPayload(DateTime scheduledDate)
        {
            ResultingState = MatchState.Scheduled;
            ScheduledDate = scheduledDate;
        }
    }
}