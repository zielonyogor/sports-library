using SportsLibrary.Model;

namespace SportsLibrary.SkiJumpingClasses
{
    /// <summary>
    /// Emitted when the technical delegate lowers the inrun gate during a competition.
    /// All subsequent jumpers receive a positive gate compensation added to their score.
    /// </summary>
    public class GateLoweredPayload : IEventPayload
    {
        public IScore? Score => null;
        /// <summary>Null when the change applies to all remaining jumpers.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The technical delegate or gate judge who ordered the change.</summary>
        public MatchSupervisor? Referee { get; init; }

        /// <summary>Gate number after lowering.</summary>
        public int NewGate { get; init; }
        /// <summary>How many gate positions were lowered.</summary>
        public int GatesLowered { get; init; }
        /// <summary>Compensation points added per jump for this gate change.</summary>
        public float CompensationPerJump { get; init; }
    }
}
