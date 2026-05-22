using System.Collections.ObjectModel;

namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a match between contestants. 
    /// </summary>
    public class Match : IMatch
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public MatchState State { get; set; } = MatchState.Scheduled;
        public ObservableCollection<IContestant> Contestants { get; set; }
        public Dictionary<IContestant, IScore> Statistics { get; set; } = new();
        public Timeline Timeline { get; set; } = new();

        public Match(string name, IEnumerable<IContestant> contestants)
        {
            Name = name;
            Contestants = new ObservableCollection<IContestant>(contestants);
        }

        public IScore? GetCurrentScore(IContestant contestant) =>
            Statistics.TryGetValue(contestant, out var score) ? score : null;

        public IContestant? GetWinner(IMatchResultStrategy? drawStrategy = null)
        {
            if (Statistics.Count == 0) return null;
            var ranked = Statistics.OrderByDescending(kv => kv.Value.GetValue()).ToList();
            if (ranked.Count >= 2 && ranked[0].Value.GetValue() == ranked[1].Value.GetValue())
                if (drawStrategy != null)
                    return drawStrategy.DetermineWinner(this);
                else
                    return ranked[0].Key; // TODO: return two contestants
            return ranked[0].Key;
        }
    }
}
