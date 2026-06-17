namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a match between contestants. The <see cref="Timeline"/> is the single source
    /// of truth — <see cref="State"/>, <see cref="Statistics"/>, and <see cref="PenaltyWinner"/>
    /// are all projections of timeline events.
    /// </summary>
    public sealed class Match
    {
        private readonly List<IContestant> _contestants;
        private readonly MatchStateTracker _stateTracker;
        private readonly MatchStatisticsTracker _statisticsTracker;
        private readonly PenaltyWinnerTracker _penaltyWinnerTracker;
        private readonly DisqualificationTracker _disqualificationTracker;
        private readonly MatchEventValidationPolicy _eventValidationPolicy;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public MatchState State => _stateTracker.CurrentState ?? throw new InvalidOperationException("Match state is not initialized yet.");
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IMatchResultStrategy ResultStrategy { get; }
        public Timeline Timeline => _timeline;
        private Timeline _timeline { get; } = new();
        public IReadOnlyDictionary<IContestant, IScore> Statistics => _statisticsTracker.Statistics;
        public IContestant? PenaltyWinner => _penaltyWinnerTracker.Winner;

        public Match(
            string name,
            IEnumerable<IContestant> contestants,
            DateTime scheduledDate,
            IMatchResultStrategy? resultStrategy = null)
        {
            Name = name;
            _contestants = new List<IContestant>(contestants);
            ResultStrategy = resultStrategy ?? new PenaltyPresentMatchResultStrategy();
            _eventValidationPolicy = new MatchEventValidationPolicy();

            _stateTracker = new MatchStateTracker();
            _statisticsTracker = new MatchStatisticsTracker();
            _penaltyWinnerTracker = new PenaltyWinnerTracker();
            _disqualificationTracker = new DisqualificationTracker();

            _timeline.Subscribe(_stateTracker);
            _timeline.Subscribe(_statisticsTracker);
            _timeline.Subscribe(_penaltyWinnerTracker);
            _timeline.Subscribe(_disqualificationTracker);

            AppendEvent(new ScheduledMatchStateEventPayload(scheduledDate), scheduledDate);
        }

        public Match(
            string name,
            IEnumerable<IContestant> contestants,
            IMatchResultStrategy? resultStrategy = null)
            : this(name, contestants, DateTime.UtcNow, resultStrategy)
        {
        }

        public void SetScore(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            RecordEvent(new ScoreSetEventPayload(contestant, score));
        }

        public void AssignPenaltyWinner(IContestant winner)
        {
            ArgumentNullException.ThrowIfNull(winner);
            if (!_contestants.Contains(winner))
                throw new ArgumentException("Penalty winner must be one of the match contestants.", nameof(winner));
            RecordEvent(new PenaltyWinnerAssignedPayload(winner));
        }

        public void Disqualify(IContestant contestant, string? reason = null)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            if (!_contestants.Contains(contestant))
                throw new ArgumentException("Disqualified contestant must be one of the match contestants.", nameof(contestant));

            RecordEvent(new DisqualificationEventPayload(contestant, reason));
        }

        public void RecordEvent(IEventPayload payload, DateTime? timestamp = null)
        {
            ArgumentNullException.ThrowIfNull(payload);
            AppendEvent(payload, timestamp ?? DateTime.UtcNow);
        }

        public void RecordEvent(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            AppendEvent(gameEvent.GetEvent(), gameEvent.Timestamp);
        }

        public void Start(DateTime? timestamp = null) =>
            AppendEvent(new InProgressMatchStateEventPayload(), timestamp ?? DateTime.UtcNow);

        public void Pause(DateTime? timestamp = null) =>
            AppendEvent(new PausedMatchStateEventPayload(), timestamp ?? DateTime.UtcNow);

        public void Finish(DateTime? timestamp = null) =>
            AppendEvent(new FinishedMatchStateEventPayload(), timestamp ?? DateTime.UtcNow);

        public void Cancel(DateTime? timestamp = null) =>
            AppendEvent(new CancelledMatchStateEventPayload(), timestamp ?? DateTime.UtcNow);

        public void Reject(string reason, DateTime? timestamp = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            AppendEvent(new RejectedMatchStateEventPayload(reason), timestamp ?? DateTime.UtcNow);
        }

        public void Reschedule(DateTime? timestamp = null) =>
            AppendEvent(new RescheduledMatchStateEventPayload(), timestamp ?? DateTime.UtcNow);

        public IScore? GetCurrentScore(IContestant contestant) =>
            _statisticsTracker.GetCurrentScore(contestant);

        public bool IsDisqualified(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            if (!_contestants.Contains(contestant))
                throw new ArgumentException("Contestant must be one of the match contestants.", nameof(contestant));

            return _disqualificationTracker.IsDisqualified(contestant);
        }

        public IReadOnlyCollection<IContestant> GetDisqualifiedContestants() =>
            _disqualificationTracker.GetDisqualifiedContestants();

        public IContestant? GetWinner() => ResultStrategy.DetermineWinner(this);

        private void AppendEvent(IEventPayload payload, DateTime timestamp)
        {
            _eventValidationPolicy.EnsureCanAppend(payload, _stateTracker.CurrentState, _contestants);

            _timeline.AddEvent(new InGameEvent(timestamp, payload));
        }
    }
}
