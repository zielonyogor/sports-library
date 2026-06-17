using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Emitted when the technical delegate raises the inrun gate during a competition.
    /// All subsequent jumpers receive gate compensation defined for this new gate.
    /// </summary>
    public sealed class GateHigherPayload : IGateChangePayload
    {
        /// <summary>Null when the change applies to all remaining jumpers.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The technical delegate or gate judge who ordered the change.</summary>
        public MatchSupervisor? Referee { get; init; }

        /// <summary>Gate number after raising.</summary>
        public int NewGate { get; init; }
        /// <summary>How many gate positions were raised.</summary>
        public int GatesRaised { get; init; }
        /// <summary>Compensation points per jump for this gate change (usually negative).</summary>
        public float CompensationPerJump { get; init; }
    }
}