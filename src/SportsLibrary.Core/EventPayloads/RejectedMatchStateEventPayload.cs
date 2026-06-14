namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Rejected.
    /// </summary>
    public class RejectedMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }
        public string Reason { get; }

        public RejectedMatchStateEventPayload(string reason)
        {
            ResultingState = MatchState.Rejected;
            Reason = reason;
        }
    }
}