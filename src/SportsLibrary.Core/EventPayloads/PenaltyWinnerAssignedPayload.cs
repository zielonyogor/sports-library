namespace SportsLibrary.Core
{
    /// <summary>
    /// Event payload recording that a penalty winner was assigned for the match.
    /// </summary>
    public sealed class PenaltyWinnerAssignedPayload : IPenaltyWinnerEventPayload
    {
        public IContestant Winner { get; }

        public PenaltyWinnerAssignedPayload(IContestant winner)
        {
            ArgumentNullException.ThrowIfNull(winner);
            Winner = winner;
        }
    }
}
