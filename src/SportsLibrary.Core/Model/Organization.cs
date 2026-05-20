namespace SportsLibrary.Core
{
    public class Organization : IOrganization
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<IContestant> Members { get; set; } = new List<IContestant>();

        public Organization(string name)
        {
            Name = name;
        }
    }
}
