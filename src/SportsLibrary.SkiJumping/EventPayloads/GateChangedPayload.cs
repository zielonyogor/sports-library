using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Emitted when the technical delegate changes the inrun gate during a competition.
    /// Positive gate compensation is typically used for lowering the gate,
    /// while negative compensation is typically used for increasing the gate.
    /// </summary>
    public sealed class GateChangedPayload : IContestantEventPayload
    {
        private int _gateDelta;

        /// <summary>Null when the change applies to all remaining jumpers.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The technical delegate or gate judge who ordered the change.</summary>
        public MatchSupervisor? Referee { get; init; }

        /// <summary>Gate number after the change.</summary>
        public int NewGate { get; init; }

        /// <summary>
        /// Signed gate delta where positive means lowered and negative means raised.
        /// For example: +2 lowered by 2 gates, -1 raised by 1 gate.
        /// </summary>
        public int GateDelta
        {
            get => _gateDelta;
            init => _gateDelta = value;
        }

        /// <summary>
        /// Backward-compatible lowering magnitude. Setting this makes <see cref="GateDelta"/> positive.
        /// </summary>
        public int GatesLowered
        {
            get => Math.Max(_gateDelta, 0);
            init => _gateDelta = Math.Abs(value);
        }

        /// <summary>
        /// Raising magnitude. Setting this makes <see cref="GateDelta"/> negative.
        /// </summary>
        public int GatesRaised
        {
            get => Math.Max(-_gateDelta, 0);
            init => _gateDelta = -Math.Abs(value);
        }

        /// <summary>True when gate was lowered, false when gate was increased.</summary>
        public bool IsLowering => _gateDelta > 0;

        /// <summary>True when gate was increased, false when gate was lowered.</summary>
        public bool IsRaising => _gateDelta < 0;

        /// <summary>
        /// Compensation points per jump for this gate change.
        /// Positive usually accompanies lowering, negative usually accompanies raising.
        /// </summary>
        public float CompensationPerJump { get; init; }
    }
}
