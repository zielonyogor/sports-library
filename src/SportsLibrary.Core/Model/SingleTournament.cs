namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a single tournament which is a set of consecutive matches between contestants, 
    /// following a specific matches strategy. 
    /// </summary>
    public sealed class SingleTournament : ITournament
    {
        private readonly List<IContestant> _contestants;
        private readonly List<Match> _matches = new();
        private readonly Dictionary<IContestant, IScore> _results = new();
        private readonly IRankingStrategy _rankingStrategy;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IReadOnlyList<Match> Matches => _matches;
        public IMatchesStrategy MatchesStrategy { get; }
        public IReadOnlyDictionary<IContestant, IScore> TournamentResults => _results;

        public SingleTournament(string name, IMatchesStrategy matchesStrategy, IRankingStrategy? rankingStrategy = null)
            : this(name, matchesStrategy, Enumerable.Empty<IContestant>(), rankingStrategy ?? new DescendingScoreRankingStrategy())
        { }

        public SingleTournament(
            string name,
            IMatchesStrategy matchesStrategy,
            IEnumerable<IContestant> contestants,
            IRankingStrategy rankingStrategy)
        {
            ArgumentNullException.ThrowIfNull(matchesStrategy);
            ArgumentNullException.ThrowIfNull(contestants);
            ArgumentNullException.ThrowIfNull(rankingStrategy);
            Name = name;
            MatchesStrategy = matchesStrategy;
            _contestants = contestants.ToList();
            _rankingStrategy = rankingStrategy;
        }

        /// <summary>
        /// Adds a contestant to this tournament.
        /// </summary>
        /// <param name="contestant">Contestant to add.</param>
        public void AddContestant(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            _contestants.Add(contestant);
        }

        /// <summary>
        /// Sets a contestant result in tournament results.
        /// </summary>
        /// <param name="contestant">Contestant to score.</param>
        /// <param name="score">Score assigned to the contestant.</param>
        public void SetResult(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            _results[contestant] = score;
        }

        /// <summary>
        /// Initializes ranking state (when available) and creates the initial match set.
        /// </summary>
        public void Start()
        {
            if (_rankingStrategy is not null)
            {
                foreach (var (contestant, score) in _rankingStrategy.InitializeScores(_contestants))
                    _results.TryAdd(contestant, score);
            }

            _matches.AddRange(MatchesStrategy.CreateMatches(_contestants));
        }

        /// <summary>
        /// Advances this single tournament to the next round using the configured matches strategy.
        /// </summary>
        /// <returns>Matches created for the new round, or an empty list when no more rounds are available.</returns>
        public IReadOnlyList<Match> AdvanceRound()
        {
            var next = MatchesStrategy.CreateNextRound(_matches);
            if (next is null) return Array.Empty<Match>();
            _matches.AddRange(next);
            return next;
        }

        /// <summary>
        /// Advances tournament state by one round.
        /// </summary>
        /// <returns><c>true</c> when a new round was created; otherwise <c>false</c>.</returns>
        public bool Advance() => AdvanceRound().Count > 0;

        /// <summary>
        /// Finalizes and orders results using ranking strategy.
        /// </summary>
        public void End()
        {
            var ranked = _rankingStrategy.Rank(new Dictionary<IContestant, IScore>(_results));
            _results.Clear();
            foreach (var (contestant, score) in ranked)
                _results[contestant] = score;
        }
    }
}
