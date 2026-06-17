namespace SportsLibrary.Core
{
    /// <summary>
    /// Marker payload for events that disqualify a contestant from winner resolution.
    /// </summary>
    public interface IDisqualificationEventPayload : IContestantEventPayload
    {
    }
}