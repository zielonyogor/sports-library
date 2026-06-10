using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballLeaderboardScore : IScore
    {
        private const int PointsForWin = 3;
        private const int PointsForDraw = 1;

        private readonly Dictionary<string, CardType> _cards = new();

        public int Rank { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int Draws { get; private set; }
        public int Points { get; private set; }
        public IReadOnlyDictionary<string, CardType> Cards => _cards;
        public int GoalsScored { get; private set; }
        public int GoalsConceded { get; private set; }
        public int GoalDifference => GoalsScored - GoalsConceded;

        public FootballLeaderboardScore() { }

        public FootballLeaderboardScore(int wins, int draws, int losses, int goalsScored = 0, int goalsConceded = 0)
        {
            if (wins < 0 || draws < 0 || losses < 0) throw new ArgumentOutOfRangeException();
            Wins = wins;
            Draws = draws;
            Losses = losses;
            GoalsScored = goalsScored;
            GoalsConceded = goalsConceded;
            Points = wins * PointsForWin + draws * PointsForDraw;
        }

        public void RegisterWin(int goalsFor = 0, int goalsAgainst = 0)
        {
            Wins++;
            Points += PointsForWin;
            GoalsScored += goalsFor;
            GoalsConceded += goalsAgainst;
        }

        public void RegisterDraw(int goalsFor = 0, int goalsAgainst = 0)
        {
            Draws++;
            Points += PointsForDraw;
            GoalsScored += goalsFor;
            GoalsConceded += goalsAgainst;
        }

        public void RegisterLoss(int goalsFor = 0, int goalsAgainst = 0)
        {
            Losses++;
            GoalsScored += goalsFor;
            GoalsConceded += goalsAgainst;
        }

        public void AddCard(string playerId, CardType card)
        {
            ArgumentException.ThrowIfNullOrEmpty(playerId);
            _cards[playerId] = card;
        }

        public void SetRank(int rank) => Rank = rank;

        public double GetValue() => Points;
    }
}
