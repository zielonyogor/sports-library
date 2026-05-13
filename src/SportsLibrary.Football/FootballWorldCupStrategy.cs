using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public class FootballWorldCupStrategy : ITournamentStrategy
    {
        private const int GroupCount = 8;
        private const int TeamsPerGroup = 4;
        private const int AdvancingPerGroup = 2;

        public List<ITournament> CreateSubTournaments(List<IContestant> contestants)
        {
            var groups = new List<ITournament>();
            for (int g = 0; g < GroupCount; g++)
            {
                var groupTeams = contestants.Skip(g * TeamsPerGroup).Take(TeamsPerGroup).ToList();
                var group = new SingleTournament($"Group {(char)('A' + g)}", new FootballGroupStageStrategy());
                group.Contestants.AddRange(groupTeams);
                groups.Add(group);
            }
            return groups;
        }

        public List<ITournament>? CreateNextStage(List<ITournament> completedTournaments)
        {
            // Only create bracket stage once (after all groups finish)
            if (completedTournaments.Any(t => t.Name == "Bracket Stage")) return null;

            var advancing = completedTournaments
                .SelectMany(t => t.TournamentResults
                    .OrderByDescending(kvp => kvp.Value.GetValue())
                    .Take(AdvancingPerGroup)
                    .Select(kvp => kvp.Key))
                .ToList();

            var bracket = new SingleTournament("Bracket Stage", new FootballBracketStageStrategy());
            bracket.Contestants.AddRange(advancing);
            return new List<ITournament> { bracket };
        }

        public Dictionary<IContestant, IScore> AggregateResults(List<ITournament> tournaments)
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
