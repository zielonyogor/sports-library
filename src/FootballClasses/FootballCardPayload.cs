using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballCardPayload : IEventPayload
    {
        public IScore? Score => null;
        /// <summary>The player who received the card.</summary>
        public IContestant? Contestant { get; init; }
        public CardType CardType { get; init; }
        public MatchSupervisor? Referee { get; init; }
        public int Minute { get; init; }
    }
}
