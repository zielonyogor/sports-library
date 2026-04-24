namespace SportsLibrary.Model
{
    public class MatchSupervisor
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Person Person { get; set; }

        public MatchSupervisor(Person person)
        {
            Person = person;
        }
    }
}
