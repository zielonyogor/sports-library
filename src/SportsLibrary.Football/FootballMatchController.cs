using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballMatchController
    {
        private readonly FootballMatchTracker _tracker;

        public FootballMatchController(Match match)
            : this(match, null)
        {
        }

        public FootballMatchController(Match match, FootballMatchTracker? tracker)
        {
            ArgumentNullException.ThrowIfNull(match);
            _tracker = tracker ?? new FootballMatchTracker(match);
        }

        public int GetGoalCount(IContestant team) => _tracker.GetGoalCount(team);

        public IReadOnlyList<FootballCardPayload> GetCards(IContestant player) => _tracker.GetCards(player);

        public bool IsPlayerSentOff(IContestant player) => _tracker.IsPlayerSentOff(player);

        public IReadOnlyList<FootballSubstitutionPayload> GetSubstitutions() => _tracker.GetSubstitutions();
    }
}
