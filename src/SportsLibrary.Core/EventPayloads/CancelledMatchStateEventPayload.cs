namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Cancelled.
    /// </summary>
    public class CancelledMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }

        public CancelledMatchStateEventPayload()
        {
            ResultingState = MatchState.Cancelled;
        }
    }
}