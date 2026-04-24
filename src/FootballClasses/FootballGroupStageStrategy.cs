using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    /// <summary>Round-robin: every team plays every other team once.</summary>
    public class FootballGroupStageStrategy : IMatchesStrategy
    {
        public List<IMatch> CreateMatches(List<IContestant> contestants)
        {
            var matches = new List<IMatch>();
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

        public List<IMatch>? CreateNextRound(List<IMatch> completedMatches) => null;
    }
}
