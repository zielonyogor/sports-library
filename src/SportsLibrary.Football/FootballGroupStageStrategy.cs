using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    /// <summary>Round-robin: every team plays every other team once.</summary>
    public sealed class FootballGroupStageStrategy : IMatchesStrategy
    {
        public IReadOnlyList<Match> CreateMatches(IReadOnlyList<IContestant> contestants)
        {
            var matches = new List<Match>();
            int matchNum = 1;
            for (int i = 0; i < contestants.Count; i++)
            {
                for (int j = i + 1; j < contestants.Count; j++)
                {
                    matches.Add(new Match($"Match {matchNum++}", new[] { contestants[i], contestants[j] }));
                }
            }
            return matches;
        }

        public IReadOnlyList<Match>? CreateNextRound(IReadOnlyList<Match> completedMatches) => null;
    }
}
