namespace SportsLibrary.Core
{
    /// <summary>
    /// Emitted when a contestant is disqualified from the match.
    /// </summary>
    public sealed class DisqualificationEventPayload : IDisqualificationEventPayload
    {
        public DisqualificationEventPayload(IContestant contestant, string? reason = null)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            Contestant = contestant;
            Reason = reason;
        }

        /// <summary>The contestant who was disqualified.</summary>
        public IContestant Contestant { get; }

        /// <summary>Optional disqualification reason.</summary>
        public string? Reason { get; }

        IContestant? IContestantEventPayload.Contestant => Contestant;
    }
}