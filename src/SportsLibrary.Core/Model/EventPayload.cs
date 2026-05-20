namespace SportsLibrary.Core
{
    public class EventPayload : IEventPayload
    {
        public IScore? Score { get; init; }
        public IContestant? Contestant { get; init; }
        public MatchSupervisor? Referee { get; init; }
    }
}
