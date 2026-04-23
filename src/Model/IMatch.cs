namespace SportsLibrary.Model
{
    public interface IMatch
    {
        public Guid Id {get;}
        public String name {get; set;}
        public DateTime date {get; set;}
        public MatchState state {get; set;}
        public ObservableCollection<IContestans> contestants {get; set;}
        public Dictionary<IContestant, IScore> statistics {get; set;}
        public Timeline timeline {get; set;}
    }
}
