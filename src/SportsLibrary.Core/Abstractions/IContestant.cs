namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing a contestant in a sports competition.
    /// <br/>
    /// A contestant can be an individual or a team, and is associated with an optional organization.
    /// </summary>
    public interface IContestant
    {
        Guid Id { get; }
        string Name { get; }
        IOrganization? Organisation { get; set; }
    }
}
