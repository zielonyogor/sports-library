namespace SportsLibrary.Model
{
    public class EventPayload : IEventPayload
    {
        public IScore? Score { get; init; }
        public IContestant? Contestant { get; init; }
        public MatchSupervisor? Referee { get; init; }
    }
}
