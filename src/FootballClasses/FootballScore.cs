using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public enum CardType { Yellow, Red }
    public enum MatchOutcome { Win, Draw, Lose }

    public class FootballMatchScore : IScore
    {
        public int GoalsScored { get; set; }
        public Dictionary<string, CardType> Cards { get; set; } = new();
        public MatchOutcome Result { get; set; }

        public double GetValue() => GoalsScored;
    }
}
