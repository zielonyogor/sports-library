using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public class FootballGoalPayload : IEventPayload
    {
        public IScore? Score { get; init; }
        /// <summary>The contestant who scored.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The contestant who provided the assist, if any.</summary>
        public IContestant? AssistProvider { get; init; }
        public MatchSupervisor? Referee { get; init; }
        /// <summary>Match minute when the goal was scored.</summary>
        public int Minute { get; init; }
    }
}
