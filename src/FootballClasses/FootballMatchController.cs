using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballMatchController(Match match)
    {
        public int GetGoalCount(IContestant team) =>
            match.Timeline.GetEventsByPayloadType<FootballGoalPayload>()
                .Count(g => g.Contestant == team);

        public IReadOnlyList<FootballCardPayload> GetCards(IContestant player) =>
            match.Timeline.GetEventsByPayloadType<FootballCardPayload>()
                .Where(c => c.Contestant == player)
                .ToList();

        public bool IsPlayerSentOff(IContestant player)
        {
            var cards = GetCards(player);
            return cards.Any(c => c.CardType == CardType.Red) ||
                   cards.Count(c => c.CardType == CardType.Yellow) >= 2;
        }

        public IReadOnlyList<FootballSubstitutionPayload> GetSubstitutions() =>
            match.Timeline.GetEventsByPayloadType<FootballSubstitutionPayload>();
    }
}
