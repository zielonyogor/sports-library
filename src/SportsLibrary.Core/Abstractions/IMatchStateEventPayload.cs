namespace SportsLibrary.Core
{
    /// <summary>
    /// Special interface representing a change in the match state.
    /// </summary>
    public interface IMatchStateEventPayload : IEventPayload
    {
        MatchState ResultingState { get; }
    }
}