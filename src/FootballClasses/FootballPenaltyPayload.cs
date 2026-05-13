using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    /// <summary>
    /// Payload for attempt at a penalty kick.
    /// </summary>
    public class FootballPenaltyPayload : IEventPayload
    {
        /// <summary>The player taking the penalty.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>Match minute for an in-play spot kick; null for shootout attempts.</summary>
        public int? Minute { get; init; }
        /// <summary>Indicates whether the penalty was scored.</summary>
        public bool Scored { get; init; }
    }
}
