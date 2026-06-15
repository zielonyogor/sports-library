namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing a strategy for creating matches in a tournament.
    /// </summary>
    public interface IMatchesStrategy
    {
        /// <summary>
        /// Creates matches based on the provided list of contestants. 
        /// The implementation of this method will depend on the specific strategy being used (e.g., single elimination, round robin, etc.).
        /// </summary>
        /// <param name="contestants">The list of contestants to be paired into matches.</param>
        /// <returns>A list of matches created from the contestants.</returns>
        IReadOnlyList<Match> CreateMatches(IReadOnlyList<IContestant> contestants);

        /// <summary>
        /// Creates the next round of matches based on the results of the completed matches.
        /// </summary>
        /// <param name="completedMatches">The list of completed matches.</param>
        /// <returns>A list of matches for the next round, or null if the final has already been created.</returns>
        IReadOnlyList<Match>? CreateNextRound(IReadOnlyList<Match> completedMatches);
    }
}
