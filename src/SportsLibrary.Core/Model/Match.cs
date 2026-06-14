namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a match between contestants. The <see cref="Timeline"/> is the single source
    /// of truth — <see cref="State"/>, <see cref="Statistics"/>, and <see cref="PenaltyWinner"/>
    /// are all projections of timeline events.
    /// </summary>
    public sealed class Match
    {
        private static readonly IMatchResultStrategy DefaultResultStrategy = new PenaltyAwareMatchResultStrategy();
        private static readonly IReadOnlyDictionary<MatchState, HashSet<MatchState>> AllowedTransitions =
            new Dictionary<MatchState, HashSet<MatchState>>
            {
                [MatchState.Scheduled] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.InProgress] = new() { MatchState.Paused, MatchState.Finished, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Paused] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.Rescheduled] = new() { MatchState.Scheduled, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Finished] = new(),
                [MatchState.Cancelled] = new(),
                [MatchState.Rejected] = new(),
            };

        private readonly List<IContestant> _contestants;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public MatchState State => Timeline.CurrentState;
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IMatchResultStrategy ResultStrategy { get; }
        public Timeline Timeline => _timeline;
        private Timeline _timeline { get; } = new();

        public IReadOnlyDictionary<IContestant, IScore> Statistics
        {
            get
            {
                var acc = new Dictionary<IContestant, IScore>();
                foreach (var ev in _timeline.Events.OrderBy(e => e.Timestamp))
                    if (ev.GetEvent() is IScoreEventPayload s && s.Contestant is { } c)
                        acc[c] = s.Apply(acc.GetValueOrDefault(c));
                return acc;
            }
        }

        public IContestant? PenaltyWinner =>
            _timeline.Events
                .OrderBy(e => e.Timestamp)
                .Select(e => e.GetEvent())
                .OfType<IPenaltyWinnerEventPayload>()
                .LastOrDefault()?.Winner;

        public Match(string name, IEnumerable<IContestant> contestants, DateTime scheduledDate, IMatchResultStrategy? resultStrategy = null)
        {
            Name = name;
            _contestants = new List<IContestant>(contestants);
            ResultStrategy = resultStrategy ?? DefaultResultStrategy;
            AppendEvent(new ScheduledMatchStateEventPayload(scheduledDate), DateTime.UtcNow, skipTransitionValidation: true);
        }

        public Match(string name, IEnumerable<IContestant> contestants, IMatchResultStrategy? resultStrategy = null)
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

        public void RecordEvent(IEventPayload payload, DateTime? timestamp = null)
        {
            ArgumentNullException.ThrowIfNull(payload);
            AppendEvent(payload, timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);
        }

        public void RecordEvent(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            AppendEvent(gameEvent.GetEvent(), gameEvent.Timestamp, skipTransitionValidation: false);
        }

        public void Start(DateTime? timestamp = null) =>
            AppendEvent(new InProgressMatchStateEventPayload(), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);

        public void Pause(DateTime? timestamp = null) =>
            AppendEvent(new PausedMatchStateEventPayload(), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);

        public void Finish(DateTime? timestamp = null) =>
            AppendEvent(new FinishedMatchStateEventPayload(), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);

        public void Cancel(DateTime? timestamp = null) =>
            AppendEvent(new CancelledMatchStateEventPayload(), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);

        public void Reject(string reason, DateTime? timestamp = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            AppendEvent(new RejectedMatchStateEventPayload(reason), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);
        }

        public void Reschedule(DateTime? timestamp = null) =>
            AppendEvent(new RescheduledMatchStateEventPayload(), timestamp ?? DateTime.UtcNow, skipTransitionValidation: false);

        public IScore? GetCurrentScore(IContestant contestant) =>
            Statistics.TryGetValue(contestant, out var score) ? score : null;

        public IContestant? GetWinner() => ResultStrategy.DetermineWinner(this);

        private void AppendEvent(IEventPayload payload, DateTime timestamp, bool skipTransitionValidation)
        {
            ValidatePayload(payload);

            if (!skipTransitionValidation && payload is IMatchStateEventPayload statePayload)
                ValidateTransition(statePayload.ResultingState);

            if (!skipTransitionValidation)
                ValidateEventAllowedInCurrentState(payload);

            _timeline.AddEvent(new InGameEvent(timestamp, payload));
        }

        private void ValidatePayload(IEventPayload payload)
        {
            if (payload is IContestantEventPayload contestantPayload && contestantPayload.Contestant is not null && !_contestants.Contains(contestantPayload.Contestant))
                throw new ArgumentException("Contestant must be one of the match contestants.", nameof(payload));

            if (payload is IPenaltyWinnerEventPayload penaltyPayload && !_contestants.Contains(penaltyPayload.Winner))
                throw new ArgumentException("Penalty winner must be one of the match contestants.", nameof(payload));
        }

        private void ValidateEventAllowedInCurrentState(IEventPayload payload)
        {
            if (payload is IMatchStateEventPayload)
                return;

            if (payload is IPenaltyWinnerEventPayload)
            {
                if (State is MatchState.InProgress or MatchState.Paused or MatchState.Finished)
                    return;

                throw new InvalidOperationException($"Cannot record a penalty resolution while the match is {State}.");
            }

            if (State is MatchState.InProgress or MatchState.Paused)
                return;

            throw new InvalidOperationException($"Cannot record match events while the match is {State}.");
        }

        private void ValidateTransition(MatchState targetState)
        {
            var currentState = State;
            if (!AllowedTransitions.TryGetValue(currentState, out var allowed) || !allowed.Contains(targetState))
                throw new InvalidOperationException($"Invalid match state transition from {currentState} to {targetState}.");
        }
    }
}
