namespace SportsLibrary.Model
{
    public class TeamContestant : IContestant
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public List<Person> Members { get; set; }
    }
}