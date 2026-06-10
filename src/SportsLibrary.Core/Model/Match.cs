namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a match between contestants.
    /// </summary>
    public sealed class Match : IMatch
    {
        private readonly List<IContestant> _contestants;
        private readonly Dictionary<IContestant, IScore> _statistics = new();

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public MatchState State { get; set; } = MatchState.Scheduled;
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IReadOnlyDictionary<IContestant, IScore> Statistics => _statistics;
        public Timeline Timeline { get; } = new();

        public IContestant? PenaltyWinner { get; private set; }

        public Match(string name, IEnumerable<IContestant> contestants)
        {
            Name = name;
            _contestants = new List<IContestant>(contestants);
        }

        public void SetScore(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            _statistics[contestant] = score;
        }

        public void AssignPenaltyWinner(IContestant winner)
        {
            ArgumentNullException.ThrowIfNull(winner);
            if (!_contestants.Contains(winner))
                throw new ArgumentException("Penalty winner must be one of the match contestants.", nameof(winner));
            PenaltyWinner = winner;
        }

        public IScore? GetCurrentScore(IContestant contestant) =>
            _statistics.TryGetValue(contestant, out var score) ? score : null;

        public IContestant? GetWinner()
        {
            if (_statistics.Count == 0) return null;
            var ranked = _statistics.OrderByDescending(kv => kv.Value.GetValue()).ToList();
            if (ranked.Count >= 2 && ranked[0].Value.GetValue() == ranked[1].Value.GetValue())
                return PenaltyWinner;
            return ranked[0].Key;
        }
    }
}
