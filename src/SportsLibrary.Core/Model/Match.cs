namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a match between contestants. The <see cref="Timeline"/> is the single source
    /// of truth — <see cref="State"/>, <see cref="Statistics"/>, and <see cref="PenaltyWinner"/>
    /// are all projections of timeline events.
    /// </summary>
    public sealed class Match : IMatch
    {
        private readonly List<IContestant> _contestants;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public MatchState State => Timeline.CurrentState;
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public Timeline Timeline { get; } = new();

        public IReadOnlyDictionary<IContestant, IScore> Statistics
        {
            get
            {
                var acc = new Dictionary<IContestant, IScore>();
                foreach (var ev in Timeline.Events.OrderBy(e => e.Timestamp))
                    if (ev.GetEvent() is IScoreEventPayload s && s.Contestant is { } c)
                        acc[c] = s.Apply(acc.GetValueOrDefault(c));
                return acc;
            }
        }

        public IContestant? PenaltyWinner =>
            Timeline.Events
                .OrderByDescending(e => e.Timestamp)
                .Select(e => e.GetEvent())
                .OfType<IPenaltyWinnerEventPayload>()
                .FirstOrDefault()?.Winner;

        public Match(string name, IEnumerable<IContestant> contestants, DateTime scheduledDate)
        {
            Name = name;
            _contestants = new List<IContestant>(contestants);
            Timeline.AddEvent(
                new InGameEvent(DateTime.UtcNow, new ScheduledMatchStateEventPayload(scheduledDate))
            );
        }

        public void SetScore(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            Timeline.AddEvent(
                new InGameEvent(DateTime.UtcNow, new ScoreSetEventPayload(contestant, score))
            );
        }

        public void AssignPenaltyWinner(IContestant winner)
        {
            ArgumentNullException.ThrowIfNull(winner);
            if (!_contestants.Contains(winner))
                throw new ArgumentException("Penalty winner must be one of the match contestants.", nameof(winner));
            Timeline.AddEvent(
                new InGameEvent(DateTime.UtcNow, new PenaltyWinnerAssignedPayload(winner))
            );
        }

        public IScore? GetCurrentScore(IContestant contestant) =>
            Statistics.TryGetValue(contestant, out var score) ? score : null;

        public IContestant? GetWinner()
        {
            var stats = Statistics;
            if (stats.Count == 0) return null;
            var ranked = stats.OrderByDescending(kv => kv.Value.GetValue()).ToList();
            if (ranked.Count >= 2 && ranked[0].Value.GetValue() == ranked[1].Value.GetValue())
                return PenaltyWinner;
            return ranked[0].Key;
        }
    }
}
