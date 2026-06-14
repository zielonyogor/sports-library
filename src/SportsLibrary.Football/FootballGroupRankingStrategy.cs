using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    /// <summary>
    /// Orders a football table by points, then goal difference, then goals scored.
    /// </summary>
    public sealed class FootballGroupRankingStrategy : IRankingStrategy
    {
        public IReadOnlyDictionary<IContestant, IScore> InitializeScores(IEnumerable<IContestant> contestants)
        {
            ArgumentNullException.ThrowIfNull(contestants);

            return contestants.ToDictionary(
                contestant => contestant,
                _ => (IScore)new FootballLeaderboardScore());
        }

        public IReadOnlyList<(IContestant Contestant, IScore Score)> Rank(IReadOnlyDictionary<IContestant, IScore> scores)
        {
            ArgumentNullException.ThrowIfNull(scores);

            var ranked = scores
                .Select(kv => (Contestant: kv.Key, Score: kv.Value as FootballLeaderboardScore))
                .Where(entry => entry.Score is not null)
                .OrderByDescending(entry => entry.Score!.Points)
                .ThenByDescending(entry => entry.Score!.GoalDifference)
                .ThenByDescending(entry => entry.Score!.GoalsScored)
                .ThenBy(entry => entry.Contestant.Name, StringComparer.Ordinal)
                .ToList();

            for (int index = 0; index < ranked.Count; index++)
                ranked[index].Score!.SetRank(index + 1);

            return ranked
                .Select(entry => (entry.Contestant, (IScore)entry.Score!))
                .ToList();
        }
    }
}