namespace SportsLibrary.Model
{
    public class SingleContestant : IContestant
    {
        public Guid Id { get; }
        public String Name { get; set; }

        public Person Person { get; set; }
    }
}