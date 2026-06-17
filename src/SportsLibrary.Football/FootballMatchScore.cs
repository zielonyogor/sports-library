using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballMatchScore : IScore
    {
        public int GoalsScored { get; private set; }

        public FootballMatchScore() { }

        public FootballMatchScore(int goalsScored)
        {
            if (goalsScored < 0) throw new ArgumentOutOfRangeException(nameof(goalsScored));
            GoalsScored = goalsScored;
        }

        public void AddGoal() => GoalsScored++;

        public double GetValue() => GoalsScored;
    }
}
