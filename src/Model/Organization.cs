namespace SportsLibrary.Model
{
    public class Organization
    {
        Guid Id { set; }
        string Name { get; set; }

        ICollection<IContestant> Members { get; set; }
    }
    
}