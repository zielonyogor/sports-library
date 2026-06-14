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
    }
}
