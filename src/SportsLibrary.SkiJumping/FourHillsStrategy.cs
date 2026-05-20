using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Creates four hill SingleTournaments (Oberstdorf, Garmisch-Partenkirchen, Innsbruck,
    /// Bischofshofen). Final results are the aggregate SkiJumpingScore across all four hills.
    /// </summary>
    public class FourHillsStrategy : ITournamentStrategy
    {
        private static readonly string[] HillNames =
        {
            "Oberstdorf Tournament",
            "Garmisch-Partenkirchen Tournament",
            "Innsbruck Tournament",
            "Bischofshofen Tournament"
        };

        private readonly IRandomProvider _random;

        public FourHillsStrategy(IRandomProvider random)
        {
            _random = random;
        }

        public List<ITournament> CreateSubTournaments(List<IContestant> contestants)
        {
            return HillNames.Select(name =>
            {
                var t = new SingleTournament(name, new SkiJumpingDuelStrategy(_random));
                t.Contestants.AddRange(contestants);
                return (ITournament)t;
            }).ToList();
        }

        public List<ITournament>? CreateNextStage(List<ITournament> completedTournaments) => null;

        public Dictionary<IContestant, IScore> AggregateResults(List<ITournament> tournaments)
        {
            var totals = new Dictionary<IContestant, double>();

            foreach (var t in tournaments)
                foreach (var (contestant, score) in t.TournamentResults)
                {
                    totals.TryGetValue(contestant, out var current);
                    totals[contestant] = current + score.GetValue();
                }

            return totals.ToDictionary(
                kvp => kvp.Key,
                kvp => (IScore)new SkiJumpingScore((float)kvp.Value, 0f, 0f, 0f));
        }
    }
}
