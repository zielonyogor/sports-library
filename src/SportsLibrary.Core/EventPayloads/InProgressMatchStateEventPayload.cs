namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to In Progress.
    /// </summary>
    public class InProgressMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }

        public InProgressMatchStateEventPayload()
        {
            ResultingState = MatchState.InProgress;
        }
    }
}