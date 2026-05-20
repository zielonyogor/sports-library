using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Pairs 50 contestants into 25 1v1 duels. After all duels finish, creates one final
    /// with 25 winners plus the 5 highest-scoring losers.
    /// </summary>
    public class SkiJumpingDuelStrategy : IMatchesStrategy
    {
        private readonly IRandomProvider _random;
        private bool _finalCreated;

        public SkiJumpingDuelStrategy(IRandomProvider random)
        {
            _random = random;
        }

        public List<IMatch> CreateMatches(List<IContestant> contestants)
        {
            _finalCreated = false;
            var shuffled = contestants.OrderBy(_ => _random.Next()).ToList();
            var matches = new List<IMatch>();
            for (int i = 0; i + 1 < shuffled.Count; i += 2)
                matches.Add(new Match($"Duel {i / 2 + 1}", new[] { shuffled[i], shuffled[i + 1] }));
            return matches;
        }

        public List<IMatch>? CreateNextRound(List<IMatch> completedMatches)
        {
            if (_finalCreated) return null;

            var winners = new List<IContestant>();
            var losers = new List<(IContestant Contestant, double Score)>();

            foreach (var match in completedMatches)
            {
                var ranked = match.Contestants
                    .Select(c => (c, match.Statistics.TryGetValue(c, out var s) ? s.GetValue() : 0d))
                    .OrderByDescending(x => x.Item2)
                    .ToList();

                if (ranked.Count >= 1) winners.Add(ranked[0].c);
                if (ranked.Count >= 2) losers.Add((ranked[1].c, ranked[1].Item2));
            }

            var top5Losers = losers
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Contestant);

            var finalists = winners.Concat(top5Losers).ToList();
            _finalCreated = true;
            return new List<IMatch> { new Match("Final", finalists) };
        }
    }
}
