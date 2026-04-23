namespace SportsLibrary.Model
{
    public class MultiTournament : ITournament
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public List<ITournament> SubTournaments { get; set; }
        public ITournamentStrategy TournamentStrategy { get; set; }
    }
}