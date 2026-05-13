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

        public IScore? GetCurrentScore(IContestant contestant) =>
            Statistics.TryGetValue(contestant, out var score) ? score : null;

        public IContestant? GetWinner()
        {
            if (Statistics.Count == 0) return null;
            var ranked = Statistics.OrderByDescending(kv => kv.Value.GetValue()).ToList();
            if (ranked.Count >= 2 && ranked[0].Value.GetValue() == ranked[1].Value.GetValue())
                return PenaltyWinner;
            return ranked[0].Key;
        }
    }
}
