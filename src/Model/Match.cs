using System.Collections.ObjectModel;

namespace SportsLibrary.Model
{
    public class Match : IMatch
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public MatchState State { get; set; } = MatchState.Scheduled;
        public ObservableCollection<IContestant> Contestants { get; set; }
        public Dictionary<IContestant, IScore> Statistics { get; set; } = new();
        public Timeline Timeline { get; set; } = new();

        /// <summary>Set externally when the match ends in a draw and penalties decide the winner.</summary>
        public IContestant? PenaltyWinner { get; set; }

        public Match(string name, IEnumerable<IContestant> contestants)
        {
            Name = name;
            Contestants = new ObservableCollection<IContestant>(contestants);
        }
    }
}
