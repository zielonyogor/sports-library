namespace SportsLibrary.Core
{
    public interface IOrganization
    {
        Guid Id { get; }
        string Name { get; set; }
        IReadOnlyCollection<IContestant> Members { get; }

        void AddMember(IContestant contestant);
        bool RemoveMember(IContestant contestant);
    }
}
