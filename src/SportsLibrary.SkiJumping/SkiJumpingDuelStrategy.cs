using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Pairs 50 contestants into 25 1v1 duels. After all duels finish, creates one final
    /// with 25 winners plus the 5 highest-scoring losers.
    /// </summary>
    public sealed class SkiJumpingDuelStrategy : IMatchesStrategy
    {
        private static readonly IMatchResultStrategy MatchResultStrategy = new HighestScoreWinsMatchResultStrategy();
        private readonly IRandomProvider _random;
        private bool _finalCreated;

        public SkiJumpingDuelStrategy(IRandomProvider random)
        {
            _random = random;
        }

        /// <summary>
        /// Pairs contestants into duels. 
        /// If the number of contestants is odd, one contestant will be left without a match and will be automatically advanced to the next round.
        /// </summary>
        /// <param name="contestants">The list of contestants to be paired into duels.</param>
        /// <returns>A list of matches created from the contestants.</returns>
        public IReadOnlyList<Match> CreateMatches(IReadOnlyList<IContestant> contestants)
        {
            _finalCreated = false;
            var shuffled = contestants.OrderBy(_ => _random.Next()).ToList();
            var matches = new List<Match>();
            for (int i = 0; i + 1 < shuffled.Count; i += 2)
                matches.Add(new Match($"Duel {i / 2 + 1}", new[] { shuffled[i], shuffled[i + 1] }, MatchResultStrategy));
            return matches;
        }

        /// <summary>
        /// Creates the next round of matches based on the results of the completed matches.
        /// </summary>
        /// <param name="completedMatches">The list of completed matches.</param>
        /// <returns>A list of matches for the next round, or null if the final has already been created.</returns>
        public IReadOnlyList<Match>? CreateNextRound(IReadOnlyList<Match> completedMatches)
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

            // lucky losers advance as well
            var top5Losers = losers
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Contestant);

            var finalists = winners.Concat(top5Losers).ToList();
            _finalCreated = true;
            return new List<Match> { new Match("Final", finalists, MatchResultStrategy) };
        }
    }
}
