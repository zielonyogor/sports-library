namespace SportsLibrary.Core
{
    public sealed class MatchSupervisor
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Person Person { get; }

        public MatchSupervisor(Person person)
        {
            Person = person;
        }
    }
}
