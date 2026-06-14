using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public sealed class SkiJumpMatchTracker : ITimelineListener
    {
        private readonly Dictionary<IContestant, double> _totals = new();
        private readonly Dictionary<IContestant, double> _bestJumps = new();
        private readonly HashSet<IContestant> _disqualified = new();

        public SkiJumpMatchTracker(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);
            match.Timeline.Subscribe(this, replayExisting: true);
        }

        public double GetTotalScore(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            return _totals.GetValueOrDefault(contestant);
        }

        public double GetBestJump(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            return _bestJumps.GetValueOrDefault(contestant);
        }

        public bool IsDisqualified(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            return _disqualified.Contains(contestant);
        }

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);

            switch (gameEvent.GetEvent())
            {
                case SkiJumpPayload { Contestant: not null } jump:
                    var points = jump.Score?.GetValue() ?? 0;
                    _totals[jump.Contestant] = _totals.GetValueOrDefault(jump.Contestant) + points;
                    _bestJumps[jump.Contestant] = Math.Max(_bestJumps.GetValueOrDefault(jump.Contestant), points);
                    break;
                case SkiJumpingDisqualificationPayload { Contestant: not null } disqualification:
                    _disqualified.Add(disqualification.Contestant);
                    break;
            }
        }
    }
}