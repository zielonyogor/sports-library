namespace SportsLibrary.Core
{
    /// <summary>
    /// Determines a winner by score first, then falls back to the match penalty winner when scores are tied.
    /// </summary>
    public sealed class PenaltyPresentMatchResultStrategy : IMatchResultStrategy
    {
        private readonly HighestScoreWinsMatchResultStrategy _inner = new();

        public IContestant? DetermineWinner(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);

            var disqualified = DisqualificationResolver.GetDisqualifiedContestants(match);

            var winner = _inner.DetermineWinner(match);
            if (winner is not null && !disqualified.Contains(winner))
                return winner;

            var ranked = match.Statistics
                .Where(kv => !disqualified.Contains(kv.Key))
                .OrderByDescending(kv => kv.Value.GetValue())
                .ToList();

            if (ranked.Count < 2)
                return ranked.FirstOrDefault().Key;

            return ranked[0].Value.GetValue() == ranked[1].Value.GetValue()
                ? (match.PenaltyWinner is not null && !disqualified.Contains(match.PenaltyWinner)
                    ? match.PenaltyWinner
                    : null)
                : ranked[0].Key;
        }
    }
}