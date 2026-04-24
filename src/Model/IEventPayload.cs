namespace SportsLibrary.Model
{
    public interface IEventPayload
    {
        IScore? Score { get; }
        IContestant? Contestant { get; }
        MatchSupervisor? Referee { get; }
    }
}
