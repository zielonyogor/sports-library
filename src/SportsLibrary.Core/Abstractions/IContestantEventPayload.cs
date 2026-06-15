namespace SportsLibrary.Core
{
    /// <summary>
    /// Marker payload for events that refer to a single contestant participating in the match.
    /// </summary>
    public interface IContestantEventPayload : IEventPayload
    {
        IContestant? Contestant { get; }
    }
}