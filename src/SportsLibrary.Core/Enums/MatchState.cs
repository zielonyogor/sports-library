namespace SportsLibrary.Core
{
    /// <summary>
    /// Enumeration representing the state of a match.
    /// </summary>
    public enum MatchState
    {
        /// <summary>
        /// The match is scheduled but has not started yet.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The match is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The match is currently paused.
        /// </summary>
        Pause,

        /// <summary>
        /// The match has finished.
        /// </summary>
        Finished,

        /// <summary>
        /// The match has been rescheduled.
        /// </summary>
        Rescheduled,

        /// <summary>
        /// The match has been cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The match has been rejected, for example due to a rule violation.
        /// </summary>
        Rejected
    }
}
