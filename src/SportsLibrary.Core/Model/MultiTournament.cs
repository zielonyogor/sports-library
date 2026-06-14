namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a composite tournament made of multiple child tournaments.
    /// </summary>
    public sealed class MultiTournament : ITournament
    {
        private readonly List<IContestant> _contestants;
        private readonly List<ITournament> _subTournaments = new();
        private readonly Dictionary<IContestant, IScore> _results = new();

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IReadOnlyList<ITournament> SubTournaments => _subTournaments;
        public ITournamentStrategy TournamentStrategy { get; }
        public IReadOnlyDictionary<IContestant, IScore> TournamentResults => _results;

        public MultiTournament(string name, ITournamentStrategy tournamentStrategy)
            : this(name, tournamentStrategy, Enumerable.Empty<IContestant>())
        { }

        public MultiTournament(string name, ITournamentStrategy tournamentStrategy, IEnumerable<IContestant> contestants)
        {
            ArgumentNullException.ThrowIfNull(tournamentStrategy);
            ArgumentNullException.ThrowIfNull(contestants);
            Name = name;
            TournamentStrategy = tournamentStrategy;
            _contestants = contestants.ToList();
        }

        /// <summary>
        /// Adds a contestant to the multi-tournament pool.
        /// </summary>
        /// <param name="contestant">Contestant to add.</param>
        public void AddContestant(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            _contestants.Add(contestant);
        }

        /// <summary>
        /// Stores or updates a direct result entry for this multi-tournament.
        /// </summary>
        /// <param name="contestant">Contestant whose score should be recorded.</param>
        /// <param name="score">Score value to assign.</param>
        public void SetResult(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            _results[contestant] = score;
        }

        /// <summary>
        /// Creates and registers initial child tournaments via the configured tournament strategy.
        /// </summary>
        public void Start()
        {
            var initial = TournamentStrategy.CreateSubTournaments(_contestants);
            _subTournaments.AddRange(initial);
            // foreach (var t in _subTournaments)
            //     t.Start();
        }

        /// <summary>
        /// Calls <see cref="ITournamentStrategy.CreateNextStage"/> to create the next competition stage 
        /// and starts any created child tournaments.
        /// </summary>
        public void AdvanceToNextStage()
        {
            var next = TournamentStrategy.CreateNextStage(_subTournaments);
            if (next == null) return;
            _subTournaments.AddRange(next);
            foreach (var t in next)
                t.Start();
        }

        /// <summary>
        /// Advances the multi-tournament by one stage.
        /// </summary>
        /// <returns><c>true</c> when at least one new child tournament is added; otherwise <c>false</c>.</returns>
        public bool Advance()
        {
            var beforeCount = _subTournaments.Count;
            AdvanceToNextStage();
            return _subTournaments.Count > beforeCount;
        }

        /// <summary>
        /// Finalizes results through strategy.
        /// </summary>
        public void End()
        {
            _results.Clear();
            foreach (var (contestant, score) in TournamentStrategy.AggregateResults(_subTournaments))
                _results[contestant] = score;
        }
    }
}
