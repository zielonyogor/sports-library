using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public class SkiJumpMatchController(Match match)
    {
        public double GetTotalScore(IContestant contestant) =>
            match.Timeline.GetEventsByPayloadType<SkiJumpPayload>()
                .Where(j => j.Contestant == contestant)
                .Sum(j => j.Score?.GetValue() ?? 0);

        public double GetBestJump(IContestant contestant) =>
            match.Timeline.GetEventsByPayloadType<SkiJumpPayload>()
                .Where(j => j.Contestant == contestant)
                .Select(j => j.Score?.GetValue() ?? 0)
                .DefaultIfEmpty(0)
                .Max();

        public bool IsDisqualified(IContestant contestant) =>
            match.Timeline.GetEventsByPayloadType<SkiJumpingDisqualificationPayload>()
                .Any(d => d.Contestant == contestant);
    }
}
