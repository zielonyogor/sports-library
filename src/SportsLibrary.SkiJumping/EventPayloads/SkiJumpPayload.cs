using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public sealed class SkiJumpPayload : IScoreEventPayload
    {
        /// <summary>The score awarded for this single jump.</summary>
        public IScore? Score { get; init; }
        public IContestant? Contestant { get; init; }
        public MatchSupervisor? Referee { get; init; }

        /// <summary>Measured distance in metres.</summary>
        public float Distance { get; init; }

        public IScore Apply(IScore? current)
        {
            var add = Score as SkiJumpingScore;
            var prev = current as SkiJumpingScore;
            if (add is null) return prev ?? new SkiJumpingScore(0, 0, 0, 0);
            if (prev is null) return add;
            return new SkiJumpingScore(
                prev.DistancePoints + add.DistancePoints,
                prev.StylePoints + add.StylePoints,
                prev.WindCompensation + add.WindCompensation,
                prev.GateCompensation + add.GateCompensation);
        }
    }
}
