using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public sealed class SkiJumpPayload : IEventPayload
    {
        public IScore? Score { get; init; }
        public IContestant? Contestant { get; init; }
        public MatchSupervisor? Referee { get; init; }

        /// <summary>Measured distance in metres.</summary>
        public float Distance { get; init; }
    }
}
