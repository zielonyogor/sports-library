namespace SportsLibrary.Core
{
    /// <summary>
    /// Default finite-state transition policy for core match lifecycle.
    /// </summary>
    public sealed class MatchStateTransitionPolicy
    {
        private static readonly IReadOnlyDictionary<MatchState, HashSet<MatchState>> AllowedTransitions =
            new Dictionary<MatchState, HashSet<MatchState>>
            {
                [MatchState.Scheduled] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.InProgress] = new() { MatchState.Paused, MatchState.Finished, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Paused] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.Rescheduled] = new() { MatchState.Scheduled, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Finished] = new(),
                [MatchState.Cancelled] = new(),
                [MatchState.Rejected] = new(),
            };

        public bool IsTransitionAllowed(MatchState currentState, MatchState targetState) =>
            AllowedTransitions.TryGetValue(currentState, out var allowed) && allowed.Contains(targetState);
    }
}