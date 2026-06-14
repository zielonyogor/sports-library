namespace SportsLibrary.Core
{
    /// <summary>
    /// Marker payload for events that assign a penalty winner to the match.
    /// </summary>
    public interface IPenaltyWinnerEventPayload : IEventPayload
    {
        IContestant Winner { get; }
    }
}
