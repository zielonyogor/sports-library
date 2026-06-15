using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Creates four hill SingleTournaments (Oberstdorf, Garmisch-Partenkirchen, Innsbruck,
    /// Bischofshofen). Final results are the aggregate SkiJumpingScore across all four hills.
    /// </summary>
    public sealed class FourHillsStrategy : ITournamentStrategy
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

        public IReadOnlyList<ITournament> CreateSubTournaments(IReadOnlyList<IContestant> contestants)
        {
            return HillNames
                .Select(name => (ITournament)new SingleTournament(
                    name,
                    new SkiJumpingDuelStrategy(_random),
                    contestants,
                    new DescendingScoreRankingStrategy()))
                .ToList();
        }

        public IReadOnlyList<ITournament>? CreateNextStage(IReadOnlyList<ITournament> completedTournaments) => null;

        public IReadOnlyDictionary<IContestant, IScore> AggregateResults(IReadOnlyList<ITournament> tournaments)
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
