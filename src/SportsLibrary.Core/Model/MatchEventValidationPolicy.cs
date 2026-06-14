namespace SportsLibrary.Core
{
    /// <summary>
    /// Validates whether an event can be appended to a match timeline.
    /// Handles payload integrity, state transitions, and event admission rules.
    /// </summary>
    public sealed class MatchEventValidationPolicy
    {
        /// <summary>
        /// Allowed state transitions for the default match lifecycle.
        /// </summary>
        private static readonly IReadOnlyDictionary<MatchState, HashSet<MatchState>> AllowedTransitions =
            new Dictionary<MatchState, HashSet<MatchState>>
            {
                [MatchState.Scheduled] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.InProgress] = new() { MatchState.Paused, MatchState.Finished, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Paused] = new() { MatchState.InProgress, MatchState.Cancelled, MatchState.Rejected, MatchState.Rescheduled },
                [MatchState.Rescheduled] = new() { MatchState.Scheduled, MatchState.Cancelled, MatchState.Rejected },
                [MatchState.Finished] = new() { MatchState.Rejected },
                [MatchState.Cancelled] = new(),
                [MatchState.Rejected] = new(),
            };

        /// <summary>
        /// Validates that the payload is consistent with match contestants and
        /// that recording it in the current state is permitted.
        /// </summary>
        /// <param name="payload">Payload to append to the timeline.</param>
        /// <param name="currentState">Current derived match state (null only before initialization).</param>
        /// <param name="contestants">Contestants that belong to the match.</param>
        /// <exception cref="ArgumentNullException">Thrown when required input is null.</exception>
        /// <exception cref="ArgumentException">Thrown when payload references contestants outside the match.</exception>
        /// <exception cref="InvalidOperationException">Thrown when transition or admission rules are violated.</exception>
        public void EnsureCanAppend(
            IEventPayload payload,
            MatchState? currentState,
            IReadOnlyCollection<IContestant> contestants)
        {
            ArgumentNullException.ThrowIfNull(payload);
            ArgumentNullException.ThrowIfNull(contestants);

            EnsurePayloadIntegrity(payload, contestants);

            if (payload is IMatchStateEventPayload statePayload)
                EnsureTransitionAllowed(currentState, statePayload.ResultingState);

            EnsureAdmissionAllowed(currentState, payload);
        }

        /// <summary>
        /// Verifies that contestant-related payload values reference only match contestants.
        /// </summary>
        private static void EnsurePayloadIntegrity(IEventPayload payload, IReadOnlyCollection<IContestant> contestants)
        {
            if (payload is IContestantEventPayload contestantPayload
                && contestantPayload.Contestant is not null
                && !contestants.Contains(contestantPayload.Contestant))
            {
                throw new ArgumentException("Contestant must be one of the match contestants.", nameof(payload));
            }

            if (payload is IPenaltyWinnerEventPayload penaltyPayload && !contestants.Contains(penaltyPayload.Winner))
                throw new ArgumentException("Penalty winner must be one of the match contestants.", nameof(payload));
        }

        /// <summary>
        /// Verifies finite-state transition rules for match-state events.
        /// </summary>
        private static void EnsureTransitionAllowed(MatchState? currentState, MatchState targetState)
        {
            if (currentState is null)
            {
                if (targetState == MatchState.Scheduled)
                    return;

                throw new InvalidOperationException($"Invalid match state transition from {currentState} to {targetState}.");
            }

            if (!AllowedTransitions.TryGetValue(currentState.Value, out var allowed) || !allowed.Contains(targetState))
                throw new InvalidOperationException($"Invalid match state transition from {currentState} to {targetState}.");
        }

        /// <summary>
        /// Verifies whether a payload type is admissible in the current match state.
        /// </summary>
        private static void EnsureAdmissionAllowed(MatchState? currentState, IEventPayload payload)
        {
            if (payload is IMatchStateEventPayload)
                return;

            if (currentState is null)
                throw new InvalidOperationException("Cannot record match events before match state is initialized.");

            if (payload is ScoreSetEventPayload && currentState == MatchState.Scheduled)
                return;

            if (payload is IPenaltyWinnerEventPayload)
            {
                if (currentState is MatchState.InProgress or MatchState.Paused or MatchState.Finished)
                    return;

                throw new InvalidOperationException($"Cannot record a penalty resolution while the match is {currentState}.");
            }

            if (currentState is MatchState.InProgress or MatchState.Paused)
                return;

            throw new InvalidOperationException($"Cannot record match events while the match is {currentState}.");
        }
    }
}