using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public sealed class SkiJumpMatchController
    {
        private readonly SkiJumpMatchTracker _tracker;

        public SkiJumpMatchController(Match match)
            : this(match, null)
        {
        }

        public SkiJumpMatchController(Match match, SkiJumpMatchTracker? tracker)
        {
            ArgumentNullException.ThrowIfNull(match);
            _tracker = tracker ?? new SkiJumpMatchTracker(match);
        }

        public double GetTotalScore(IContestant contestant) => _tracker.GetTotalScore(contestant);

        public double GetBestJump(IContestant contestant) => _tracker.GetBestJump(contestant);

        public bool IsDisqualified(IContestant contestant) => _tracker.IsDisqualified(contestant);

        /// <summary>
        /// Gets the current gate compensation that applies to subsequent jumpers.
        /// </summary>
        public float GetCurrentGateCompensation() => _tracker.GetCurrentGateCompensation();

        /// <summary>
        /// Gets the history of all gate changes that occurred during the match.
        /// </summary>
        public IReadOnlyList<GateLoweredPayload> GetGateChangeHistory() => _tracker.GetGateChangeHistory();

        /// <summary>
        /// Creates a SkiJumpingScore with the current gate compensation automatically applied.
        /// This connects gate changes to jump scores without manual compensation entry.
        /// </summary>
        public SkiJumpingScore CreateScoreWithCurrentGateCompensation(
            float distancePoints, 
            float stylePoints, 
            float windCompensation = 0f)
        {
            return new SkiJumpingScore(
                distancePoints,
                stylePoints,
                windCompensation,
                GetCurrentGateCompensation());
        }

        /// <summary>
        /// Creates a jump payload with a score that already includes current gate compensation.
        /// </summary>
        public SkiJumpPayload CreateJumpPayload(
            IContestant contestant,
            float distance,
            float distancePoints,
            float stylePoints,
            float windCompensation = 0f,
            MatchSupervisor? referee = null)
        {
            ArgumentNullException.ThrowIfNull(contestant);

            return new SkiJumpPayload
            {
                Contestant = contestant,
                Referee = referee,
                Distance = distance,
                Score = CreateScoreWithCurrentGateCompensation(distancePoints, stylePoints, windCompensation),
            };
        }
    }
}
