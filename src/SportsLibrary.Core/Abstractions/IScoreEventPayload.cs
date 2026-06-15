namespace SportsLibrary.Core
{
    /// <summary>
    /// Marker payload for events that contribute to a contestant's score.
    /// The <see cref="Match.Statistics"/> projection folds these in timeline order.
    /// </summary>
    public interface IScoreEventPayload : IContestantEventPayload
    {
        /// <summary>Produces the contestant's new score given their previous score (null if none yet).</summary>
        IScore Apply(IScore? current);
    }
}
