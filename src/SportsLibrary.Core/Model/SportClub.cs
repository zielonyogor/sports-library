namespace SportsLibrary.Core
{
    public sealed class SportClub : Organization
    {
        public CountryCode Country { get; set; }

        public SportClub(string name, CountryCode country) : base(name)
        {
            Country = country;
        }
    }
}
