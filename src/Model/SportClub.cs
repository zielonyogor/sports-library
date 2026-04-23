namespace SportsLibrary.Model
{
    public class SportClub : Organization
    {
        Guid Id { set; }
        string Name { get; set; }
        CountryCode Country { get; set; }
    }
}