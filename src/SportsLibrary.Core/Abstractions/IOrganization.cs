namespace SportsLibrary.Core
{
    public interface IOrganization
    {
        Guid Id { get; }
        string Name { get; set; }
        ICollection<IContestant> Members { get; set; }
    }
}
