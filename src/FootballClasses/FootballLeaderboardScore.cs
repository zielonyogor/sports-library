using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballLeaderboardScore : IScore
    {
        public int Rank { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Points { get; set; }
        public Dictionary<string, CardType> Cards { get; set; } = new();
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int GoalDifference => GoalsScored - GoalsConceded;

        public double GetValue() => Points;
    }
}
