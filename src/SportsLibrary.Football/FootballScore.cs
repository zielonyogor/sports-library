using SportsLibrary.Core;

namespace SportsLibrary.Football
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
