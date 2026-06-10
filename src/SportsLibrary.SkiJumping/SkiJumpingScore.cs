using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public sealed class SkiJumpingScore : IScore
    {
        public float DistancePoints { get; }
        public float StylePoints { get; }
        public float WindCompensation { get; }
        public float GateCompensation { get; }

        public float Points => DistancePoints + StylePoints + WindCompensation + GateCompensation;

        public SkiJumpingScore(float distancePoints, float stylePoints, float windCompensation, float gateCompensation)
        {
            DistancePoints = distancePoints;
            StylePoints = stylePoints;
            WindCompensation = windCompensation;
            GateCompensation = gateCompensation;
        }

        public double GetValue() => Points;
    }
}
