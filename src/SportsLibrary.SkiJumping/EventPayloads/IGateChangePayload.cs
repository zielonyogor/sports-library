using SportsLibrary.Core;

namespace SportsLibrary.SkiJumping
{
    /// <summary>
    /// Common contract for ski jumping gate change events.
    /// </summary>
    public interface IGateChangePayload : IContestantEventPayload
    {
        int NewGate { get; }
        float CompensationPerJump { get; }
    }
}