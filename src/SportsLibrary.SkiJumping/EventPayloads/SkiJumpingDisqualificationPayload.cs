using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Emitted when a contestant is disqualified during a ski jumping competition.
    /// </summary>
    public class SkiJumpingDisqualificationPayload : IEventPayload
    {
        /// <summary>The contestant who was disqualified.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The reason for the disqualification.</summary>
        public string? Reason { get; init; }
    }
}
