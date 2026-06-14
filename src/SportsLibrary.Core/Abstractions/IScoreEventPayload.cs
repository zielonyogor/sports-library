namespace SportsLibrary.Core
{
    /// <summary>
    /// Marker payload for events that contribute to a contestant's score.
    /// The <see cref="Match.Statistics"/> projection folds these in timeline order.
    /// </summary>
    public interface IScoreEventPayload : IEventPayload
    {
        /// <summary>The contestant this score event applies to. Null means the event carries no scoring effect and is skipped by the projection.</summary>
        IContestant? Contestant { get; }

        /// <summary>Produces the contestant's new score given their previous score (null if none yet).</summary>
        IScore Apply(IScore? current);
    }
}
