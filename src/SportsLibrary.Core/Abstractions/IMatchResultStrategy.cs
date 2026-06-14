namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing a strategy for determining the winner of a match.
    /// </summary>
    public interface IMatchResultStrategy
    {
        /// <summary>
        /// Determines the winner of the given match.
        /// </summary>
        /// <param name="match">The match for which to determine the winner.</param>
        /// <returns>The winning contestant, or null if there is no winner.</returns>
        IContestant? DetermineWinner(Match match);
    }
}
