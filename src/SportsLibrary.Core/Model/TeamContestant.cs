namespace SportsLibrary.Core
{
    public class TeamContestant : IContestant
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public IOrganization? Organisation { get; set; }
        public List<Person> Members { get; set; } = new();

        public TeamContestant(string name)
        {
            Name = name;
        }
    }
}
