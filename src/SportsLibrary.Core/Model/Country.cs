namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a country, which is a specific type of organization.
    /// </summary>
    public sealed class Country : Organization
    {
        public CountryCode Code { get; }
        public Country(string name, CountryCode code) : base(name)
        {
            Code = code;
        }

        // any additional properties or methods specific to Country could be added here
    }
}
