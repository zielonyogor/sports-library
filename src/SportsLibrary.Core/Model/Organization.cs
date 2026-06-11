namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents an organization, which can be associated with members (contestants).
    /// Each organization has a unique identifier and a name.
    /// </summary>
    public abstract class Organization : IOrganization
    {
        private readonly List<IContestant> _members = new();

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public IReadOnlyCollection<IContestant> Members => _members;

        protected Organization(string name)
        {
            Name = name;
        }

        public void AddMember(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            _members.Add(contestant);
        }

        public bool RemoveMember(IContestant contestant) => _members.Remove(contestant);
    }
}
