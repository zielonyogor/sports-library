using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballMatchScore : IScore
    {
        private readonly Dictionary<string, CardType> _cards = new();

        public int GoalsScored { get; private set; }
        public IReadOnlyDictionary<string, CardType> Cards => _cards;
        public MatchOutcome Result { get; private set; }

        public FootballMatchScore() { }

        public FootballMatchScore(int goalsScored, MatchOutcome result = MatchOutcome.Draw)
        {
            if (goalsScored < 0) throw new ArgumentOutOfRangeException(nameof(goalsScored));
            GoalsScored = goalsScored;
            Result = result;
        }

        public void AddGoal() => GoalsScored++;

        public void AddCard(string playerId, CardType card)
        {
            ArgumentException.ThrowIfNullOrEmpty(playerId);
            _cards[playerId] = card;
        }

        public void SetResult(MatchOutcome result) => Result = result;

        public double GetValue() => GoalsScored;
    }
}
