using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    public class SkiJumpingScore : IScore
    {
        public float DistancePoints { get; set; }
        public float StylePoints { get; set; }
        public float WindCompensation { get; set; }
        public float GateCompensation { get; set; }

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
