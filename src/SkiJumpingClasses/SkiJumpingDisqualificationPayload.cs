using SportsLibrary.Model;

namespace SportsLibrary.SkiJumpingClasses
{
    public class SkiJumpingDisqualificationPayload : IEventPayload
    {
        public IContestant? Contestant { get; init; }
        public string? Reason { get; init; }
    }
}
