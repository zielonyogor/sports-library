namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface that defines how contestants are ranked.
    /// </summary>
    public interface IRankingStrategy
    {
        /// <summary>
        /// Creates initial scores for all contestants.
        /// </summary>
        /// <param name="contestants">Participating contestants to be ranked.</param>
        /// <returns>
        /// A read-only dictionary of contestants with their initial scores, like zero.
        /// </returns>
        IReadOnlyDictionary<IContestant, IScore> InitializeScores(IEnumerable<IContestant> contestants);

        /// <summary>
        /// Produces an ordered ranking from the provided contestants and their scores.
        /// </summary>
        /// <param name="scores">Contestant-score dictionary</param>
        /// <returns>
        /// Contestants ordered from highest to lowest position together with their scores.
        /// </returns>
        IReadOnlyList<(IContestant Contestant, IScore Score)> Rank(IReadOnlyDictionary<IContestant, IScore> scores);
    }
}
