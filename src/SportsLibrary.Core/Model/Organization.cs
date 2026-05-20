namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents an organization, which can be associated with members (contestants). 
    /// Each organization has a unique identifier and a name.
    /// </summary>
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
