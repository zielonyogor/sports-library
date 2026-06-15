namespace SportsLibrary.Core
{
    /// <summary>
    /// Determines a winner by taking the contestant with the highest projected score.
    /// Equal top scores produce no winner.
    /// </summary>
    public sealed class HighestScoreWinsMatchResultStrategy : IMatchResultStrategy
    {
        public IContestant? DetermineWinner(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);

            var ranked = match.Statistics
                .OrderByDescending(kv => kv.Value.GetValue())
                .ToList();

            if (ranked.Count == 0)
                return null;

            if (ranked.Count > 1 && ranked[0].Value.GetValue() == ranked[1].Value.GetValue())
                return null;

            return ranked[0].Key;
        }
    }
}