namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a team contestant, which consists of multiple members (persons) and is associated with an optional organization.
    /// </summary>
    public sealed class TeamContestant : IContestant
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
