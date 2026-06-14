namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Rescheduled.
    /// </summary>
    public class RescheduledMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }

        public RescheduledMatchStateEventPayload()
        {
            ResultingState = MatchState.Rescheduled;
        }
    }
}