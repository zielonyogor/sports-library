using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballerInjuredPayload : IEventPayload
    {
        public IScore? Score => null;
        /// <summary>The player who was injured.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The official who stopped play.</summary>
        public MatchSupervisor? Referee { get; init; }
        public int Minute { get; init; }
        public string? Description { get; init; }
    }
}
