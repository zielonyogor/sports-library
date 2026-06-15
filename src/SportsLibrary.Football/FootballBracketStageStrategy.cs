using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    /// <summary>Single-elimination bracket. Draws resolved by PenaltyWinner on the concrete Match.</summary>
    public sealed class FootballBracketStageStrategy : IMatchesStrategy
    {
        private readonly IMatchResultStrategy _matchResultStrategy;
        private int _roundNumber;

        public FootballBracketStageStrategy(IMatchResultStrategy? matchResultStrategy = null)
        {
            _matchResultStrategy = matchResultStrategy ?? new PenaltyPresentMatchResultStrategy();
        }

        public IReadOnlyList<Match> CreateMatches(IReadOnlyList<IContestant> contestants)
        {
            _roundNumber = 1;
            return CreateRoundMatches(contestants);
        }

        public IReadOnlyList<Match>? CreateNextRound(IReadOnlyList<Match> completedMatches)
        {
            var winners = completedMatches
                .Select(match => match.GetWinner())
                .Where(w => w != null)
                .Select(w => w!)
                .ToList();

            if (winners.Count <= 1) return null;

            _roundNumber++;
            return CreateRoundMatches(winners);
        }

        private List<Match> CreateRoundMatches(IReadOnlyList<IContestant> contestants)
        {
            var matches = new List<Match>();
            for (int i = 0; i + 1 < contestants.Count; i += 2)
            {
                matches.Add(new Match($"Round {_roundNumber} Match {i / 2 + 1}",
                    new[] { contestants[i], contestants[i + 1] },
                    _matchResultStrategy));
            }
            return matches;
        }
    }
}
