namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Finished.
    /// </summary>
    public class FinishedMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }

        public FinishedMatchStateEventPayload()
        {
            ResultingState = MatchState.Finished;
        }
    }
}