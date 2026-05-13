using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public class FootballerInjuredPayload : IEventPayload
    {
        /// <summary>The player who was injured.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The referee who stopped play.</summary>
        public MatchSupervisor? Referee { get; init; }
        /// <summary>Match minute when the injury occurred.</summary>
        public int Minute { get; init; }
        /// <summary>Additional description of the injury.</summary>
        public string? Description { get; init; }
    }
}
