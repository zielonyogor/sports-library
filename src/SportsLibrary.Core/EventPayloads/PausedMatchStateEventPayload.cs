namespace SportsLibrary.Core
{
    /// <summary>
    /// Class representing the payload of a match state changed to Paused.
    /// </summary>
    public class PausedMatchStateEventPayload : IMatchStateEventPayload
    {
        public MatchState ResultingState { get; }

        public PausedMatchStateEventPayload()
        {
            ResultingState = MatchState.Paused;
        }
    }
}