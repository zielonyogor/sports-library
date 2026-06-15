using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballMatchTracker : ITimelineListener
    {
        private readonly Dictionary<IContestant, int> _goals = new();
        private readonly Dictionary<IContestant, List<FootballCardPayload>> _cards = new();
        private readonly List<FootballSubstitutionPayload> _substitutions = new();

        public FootballMatchTracker(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);
            match.Timeline.Subscribe(this, replayExisting: true);
        }

        public int GetGoalCount(IContestant team)
        {
            ArgumentNullException.ThrowIfNull(team);
            return _goals.GetValueOrDefault(team);
        }

        public IReadOnlyList<FootballCardPayload> GetCards(IContestant player)
        {
            ArgumentNullException.ThrowIfNull(player);
            return _cards.GetValueOrDefault(player) ?? [];
        }

        public bool IsPlayerSentOff(IContestant player)
        {
            ArgumentNullException.ThrowIfNull(player);
            var cards = GetCards(player);
            return cards.Any(c => c.CardType == CardType.Red) ||
                   cards.Count(c => c.CardType == CardType.Yellow) >= 2;
        }

        public IReadOnlyList<FootballSubstitutionPayload> GetSubstitutions() => _substitutions;

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);

            switch (gameEvent.GetEvent())
            {
                case FootballGoalPayload { Contestant: not null } goal:
                    _goals[goal.Contestant] = _goals.GetValueOrDefault(goal.Contestant) + 1;
                    break;
                case FootballCardPayload { Contestant: not null } card:
                    if (!_cards.TryGetValue(card.Contestant, out var entries))
                    {
                        entries = new List<FootballCardPayload>();
                        _cards[card.Contestant] = entries;
                    }

                    entries.Add(card);
                    break;
                case FootballSubstitutionPayload substitution:
                    _substitutions.Add(substitution);
                    break;
            }
        }
    }
}