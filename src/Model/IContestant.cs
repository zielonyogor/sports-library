namespace SportsLibrary
{
    public interface IContestant
    {
        public Guid Id { get; }
        public String Name { get; set; }
    }
}