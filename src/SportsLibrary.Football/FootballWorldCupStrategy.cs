using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballWorldCupStrategy : ITournamentStrategy
    {
        private const int GroupCount = 8;
        private const int TeamsPerGroup = 4;
        private const int AdvancingPerGroup = 2;
        private static readonly FootballGroupRankingStrategy GroupRanking = new();

        public IReadOnlyList<ITournament> CreateSubTournaments(IReadOnlyList<IContestant> contestants)
        {
            var groups = new List<ITournament>();
            for (int g = 0; g < GroupCount; g++)
            {
                var groupTeams = contestants.Skip(g * TeamsPerGroup).Take(TeamsPerGroup);
                var group = new SingleTournament(
                    $"Group {(char)('A' + g)}",
                    new FootballGroupStageStrategy(),
                    groupTeams,
                    new FootballGroupRankingStrategy());
                groups.Add(group);
            }
            return groups;
        }

        public IReadOnlyList<ITournament>? CreateNextStage(IReadOnlyList<ITournament> completedTournaments)
        {
            // Only create bracket stage once (after all groups finish)
            if (completedTournaments.Any(t => t.Name == "Bracket Stage")) return null;

            var advancing = completedTournaments
                .SelectMany(t => GroupRanking
                    .Rank(t.TournamentResults)
                    .Take(AdvancingPerGroup)
                    .Select(entry => entry.Contestant))
                .ToList();

            var bracket = new SingleTournament(
                "Bracket Stage",
                new FootballBracketStageStrategy(),
                advancing,
                new DescendingScoreRankingStrategy());
            return new List<ITournament> { bracket };
        }

        public IReadOnlyDictionary<IContestant, IScore> AggregateResults(IReadOnlyList<ITournament> tournaments)
        {
            var bracketStage = tournaments.FirstOrDefault(t => t.Name == "Bracket Stage");
            if (bracketStage != null && bracketStage.TournamentResults.Count > 0)
                return new Dictionary<IContestant, IScore>(bracketStage.TournamentResults);

            // Fall back to merged group results
            var results = new Dictionary<IContestant, IScore>();
            foreach (var t in tournaments)
                foreach (var (contestant, score) in t.TournamentResults)
                    results[contestant] = score;
            return results;
        }
    }
}
