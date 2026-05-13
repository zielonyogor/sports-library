namespace SportsLibrary.Core
{
    public interface IContestant
    {
        Guid Id { get; }
        string Name { get; set; }
        IOrganization? Organisation { get; set; }
    }
}
