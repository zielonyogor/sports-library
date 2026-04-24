namespace SportsLibrary.Model
{
    public class SingleTournament : ITournament
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public List<IContestant> Contestants { get; set; } = new();
        public List<IMatch> Matches { get; set; } = new();
        public IMatchesStrategy MatchesStrategy { get; set; }
        public Dictionary<IContestant, IScore> TournamentResults { get; set; } = new();

        public SingleTournament(string name, IMatchesStrategy matchesStrategy)
        {
            Name = name;
            MatchesStrategy = matchesStrategy;
        }

        public void Start()
        {
            Matches.AddRange(MatchesStrategy.CreateMatches(Contestants));
        }

        public void End()
        {
            // Results are populated externally as matches complete.
            // Call MatchesStrategy.CreateNextRound to progress to subsequent rounds.
        }
    }
}
