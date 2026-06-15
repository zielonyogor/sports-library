namespace SportsLibrary.Core
{
    /// <summary>
    /// Generic score event that replaces a contestant's current score wholesale.
    /// Used by <see cref="Match.SetScore"/>; sport-specific incremental events
    /// (e.g. a goal) should implement <see cref="IScoreEventPayload"/> directly.
    /// </summary>
    public sealed class ScoreSetEventPayload : IScoreEventPayload
    {
        public IContestant Contestant { get; }
        public IScore Score { get; }

        public ScoreSetEventPayload(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            Contestant = contestant;
            Score = score;
        }

        public IScore Apply(IScore? current) => Score;
    }
}
