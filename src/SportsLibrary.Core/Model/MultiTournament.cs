namespace SportsLibrary.Core
{
    public class MultiTournament : ITournament
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public List<ITournament> SubTournaments { get; set; } = new();
        public ITournamentStrategy TournamentStrategy { get; set; }
        public List<IContestant> Contestants { get; set; } = new();
        public Dictionary<IContestant, IScore> TournamentResults { get; set; } = new();

        public MultiTournament(string name, ITournamentStrategy tournamentStrategy)
        {
            Name = name;
            TournamentStrategy = tournamentStrategy;
        }

        public void Start()
        {
            var initial = TournamentStrategy.CreateSubTournaments(Contestants);
            SubTournaments.AddRange(initial);
            foreach (var t in SubTournaments)
                t.Start();
        }

        public void AdvanceToNextStage()
        {
            var next = TournamentStrategy.CreateNextStage(SubTournaments);
            if (next == null) return;
            SubTournaments.AddRange(next);
            foreach (var t in next)
                t.Start();
        }

        public void End()
        {
            TournamentResults = TournamentStrategy.AggregateResults(SubTournaments);
        }
    }
}
