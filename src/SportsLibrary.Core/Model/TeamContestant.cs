namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a team contestant, which consists of multiple members (persons) and is associated with an optional organization.
    /// </summary>
    public sealed class TeamContestant : IContestant
    {
        private readonly List<Person> _members = new();

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public IOrganization? Organisation { get; set; }
        public IReadOnlyList<Person> Members => _members;

        public TeamContestant(string name)
        {
            Name = name;
        }

        public void AddMember(Person person)
        {
            ArgumentNullException.ThrowIfNull(person);
            _members.Add(person);
        }

        public bool RemoveMember(Person person) => _members.Remove(person);
    }
}
