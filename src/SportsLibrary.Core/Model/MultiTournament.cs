namespace SportsLibrary.Core
{
    public sealed class MultiTournament : ITournament, IStageAdvancingTournament
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
            _contestants = new List<IContestant>(contestants);
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
            var initial = TournamentStrategy.CreateSubTournaments(_contestants);
            _subTournaments.AddRange(initial);
            foreach (var t in _subTournaments)
                t.Start();
        }

        public void AdvanceToNextStage()
        {
            var next = TournamentStrategy.CreateNextStage(_subTournaments);
            if (next == null) return;
            _subTournaments.AddRange(next);
            foreach (var t in next)
                t.Start();
        }

        public bool Advance()
        {
            var beforeCount = _subTournaments.Count;
            AdvanceToNextStage();
            return _subTournaments.Count > beforeCount;
        }

        public void End()
        {
            _results.Clear();
            foreach (var (contestant, score) in TournamentStrategy.AggregateResults(_subTournaments))
                _results[contestant] = score;
        }
    }
}
