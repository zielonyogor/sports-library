using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    /// <summary>Single-elimination bracket. Draws resolved by PenaltyWinner on the concrete Match.</summary>
    public class FootballBracketStageStrategy : IMatchesStrategy
    {
        private int _roundNumber;

        public List<IMatch> CreateMatches(List<IContestant> contestants)
        {
            _roundNumber = 1;
            return CreateRoundMatches(contestants);
        }

        public List<IMatch>? CreateNextRound(List<IMatch> completedMatches)
        {
            var winners = completedMatches
                .Select(GetMatchWinner)
                .Where(w => w != null)
                .Select(w => w!)
                .ToList();

            if (winners.Count <= 1) return null;

            _roundNumber++;
            return CreateRoundMatches(winners);
        }

        private List<IMatch> CreateRoundMatches(List<IContestant> contestants)
        {
            var matches = new List<IMatch>();
            for (int i = 0; i + 1 < contestants.Count; i += 2)
            {
                matches.Add(new Match($"Round {_roundNumber} Match {i / 2 + 1}",
                    new[] { contestants[i], contestants[i + 1] }));
            }
            return matches;
        }

        private static IContestant? GetMatchWinner(IMatch match)
        {
            var scored = match.Contestants
                .Select(c => (c, match.Statistics.TryGetValue(c, out var s) ? s.GetValue() : 0d))
                .OrderByDescending(x => x.Item2)
                .ToList();

            if (scored.Count < 2) return scored.FirstOrDefault().c;

            if (scored[0].Item2 == scored[1].Item2)
            {
                // Draw — check for a penalty winner on the concrete Match type
                return match is Match m ? m.PenaltyWinner : scored[0].c;
            }

            return scored[0].c;
        }
    }
}
