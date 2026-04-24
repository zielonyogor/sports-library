namespace SportsLibrary.Model
{
    public class SingleContestant : IContestant
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public IOrganization? Organisation { get; set; }
        public Person Person { get; set; }

        public SingleContestant(string name, Person person)
        {
            Name = name;
            Person = person;
        }
    }
}
