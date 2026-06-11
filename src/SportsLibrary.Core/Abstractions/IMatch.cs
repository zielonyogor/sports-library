namespace SportsLibrary.Core
{
    public interface IMatch
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public MatchState State { get; set; }
        public IReadOnlyList<IContestant> Contestants { get; }
        public IReadOnlyDictionary<IContestant, IScore> Statistics { get; }
        public Timeline Timeline { get; }

        public void SetScore(IContestant contestant, IScore score);
    }
}
