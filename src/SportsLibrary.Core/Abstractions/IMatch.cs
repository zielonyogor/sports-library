using System.Collections.ObjectModel;

namespace SportsLibrary.Core
{
    public interface IMatch
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public DateTime Date  { get; set; }
        public MatchState State { get; set; }
        public ObservableCollection<IContestant> Contestants { get; set; }
        public Dictionary<IContestant, IScore> Statistics { get; set; }
        public Timeline Timeline { get; set; }
    }
}
