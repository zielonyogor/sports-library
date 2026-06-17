using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballGoalPayload : IScoreEventPayload
    {
        /// <summary>Optional snapshot of the team's score at the moment of the goal. Informational only — <see cref="Apply"/> ignores it.</summary>
        public IScore? Score { get; init; }
        /// <summary>The contestant who scored.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The contestant who provided the assist, if any.</summary>
        public IContestant? AssistProvider { get; init; }
        public MatchSupervisor? Referee { get; init; }
        /// <summary>Match minute when the goal was scored.</summary>
        public int Minute { get; init; }

        public IScore Apply(IScore? current)
        {
            var prev = current as FootballMatchScore;
            return new FootballMatchScore(goalsScored: (prev?.GoalsScored ?? 0) + 1);
        }
    }
}
