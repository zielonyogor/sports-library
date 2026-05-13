using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballCardPayload : IEventPayload
    {
        /// <summary>The player who received the card.</summary>
        public required IContestant Contestant { get; init; }
        /// <summary>The type of the card (yellow or red).</summary>
        public CardType CardType { get; init; }
        /// <summary>The referee who issued the card.</summary>
        public required MatchSupervisor Referee { get; init; }
        /// <summary>Match minute when the card was issued.</summary>
        public int Minute { get; init; }
    }
}
