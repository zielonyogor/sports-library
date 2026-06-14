namespace SportsLibrary.Core
{
    /// <summary>
    /// Orders contestants by descending score value and leaves score instances unchanged.
    /// </summary>
    public sealed class DescendingScoreRankingStrategy : IRankingStrategy
    {
        public IReadOnlyDictionary<IContestant, IScore> InitializeScores(IEnumerable<IContestant> contestants)
        {
            ArgumentNullException.ThrowIfNull(contestants);
            return new Dictionary<IContestant, IScore>();
        }

        public IReadOnlyList<(IContestant Contestant, IScore Score)> Rank(IReadOnlyDictionary<IContestant, IScore> scores)
        {
            ArgumentNullException.ThrowIfNull(scores);

            return scores
                .OrderByDescending(kv => kv.Value.GetValue())
                .ThenBy(kv => kv.Key.Name, StringComparer.Ordinal)
                .Select(kv => (kv.Key, kv.Value))
                .ToList();
        }
    }
}