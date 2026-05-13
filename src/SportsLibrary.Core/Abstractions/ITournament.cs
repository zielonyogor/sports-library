namespace SportsLibrary.Core
{
    public interface ITournament
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public List<IContestant> Contestants { get; set; }
        public Dictionary<IContestant, IScore> TournamentResults { get; set; }

        public void Start();
        public void End();
    }
}