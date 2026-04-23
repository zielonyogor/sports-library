namespace SportsLibrary.Model
{
    public interface ITournamentStrategy
    {
        public Guid Id { get; }
        public ITournament NextTournament();
    }
}