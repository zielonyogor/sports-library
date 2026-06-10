namespace SportsLibrary.Core
{
    public sealed class SingleContestant : IContestant
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public IOrganization? Organisation { get; set; }
        public Person Person { get; }

        public SingleContestant(string name, Person person)
        {
            Name = name;
            Person = person;
        }
    }
}
