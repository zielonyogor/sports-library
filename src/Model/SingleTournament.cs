namespace SportsLibrary.Model
{
    public class SingleTournament : ITournament
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public List<IContestant> Contestants { get; set; }
        public List<IMatch> Matches { get; set; }
        public IMatchesStrategy MatchesStrategy { get; set; }
    }
}