namespace SportsLibrary.Core
{
    public sealed class SingleTournament : ITournament
    {
        private readonly List<IContestant> _contestants;
        private readonly List<IMatch> _matches = new();
        private readonly Dictionary<IContestant, IScore> _results = new();
        private readonly IRankingStrategy? _rankingStrategy;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public IReadOnlyList<IContestant> Contestants => _contestants;
        public IReadOnlyList<IMatch> Matches => _matches;
        public IMatchesStrategy MatchesStrategy { get; }
        public IReadOnlyDictionary<IContestant, IScore> TournamentResults => _results;

        public SingleTournament(string name, IMatchesStrategy matchesStrategy)
            : this(name, matchesStrategy, Enumerable.Empty<IContestant>(), rankingStrategy: null)
        { }

        public SingleTournament(
            string name,
            IMatchesStrategy matchesStrategy,
            IEnumerable<IContestant> contestants,
            IRankingStrategy? rankingStrategy = null)
        {
            ArgumentNullException.ThrowIfNull(matchesStrategy);
            ArgumentNullException.ThrowIfNull(contestants);
            Name = name;
            MatchesStrategy = matchesStrategy;
            _contestants = new List<IContestant>(contestants);
            _rankingStrategy = rankingStrategy;
        }

        public void AddContestant(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            _contestants.Add(contestant);
        }

        public void SetResult(IContestant contestant, IScore score)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            ArgumentNullException.ThrowIfNull(score);
            _results[contestant] = score;
        }

        public void Start()
        {
            _matches.AddRange(MatchesStrategy.CreateMatches(_contestants));
        }

        public IReadOnlyList<IMatch> AdvanceRound()
        {
            var next = MatchesStrategy.CreateNextRound(_matches);
            if (next is null) return Array.Empty<IMatch>();
            _matches.AddRange(next);
            return next;
        }

        public void End()
        {
            if (_rankingStrategy is null) return;
            var ranked = _rankingStrategy.Rank(new Dictionary<IContestant, IScore>(_results));
            _results.Clear();
            foreach (var (contestant, score) in ranked)
                _results[contestant] = score;
        }
    }
}
