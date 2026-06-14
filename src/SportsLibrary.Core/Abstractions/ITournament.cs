namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing a tournament. A tournament is a collection of matches that are played between contestants.
    /// The tournament is responsible for managing the matches, tracking the contestants and determining the winner of the tournament.
    /// </summary>
    public interface ITournament
    {
        public Guid Id { get; }
        public string Name { get; }

        /// <summary>
        /// List of contestants participating in the tournament. 
        /// Editable because contestants can be added to the tournament after its creation (f.e. after announcement of the tournament).
        /// </summary>
        public IReadOnlyList<IContestant> Contestants { get; }
        public IReadOnlyDictionary<IContestant, IScore> TournamentResults { get; }

        /// <summary>
        /// Adds a contestant to the tournament roster.
        /// </summary>
        /// <param name="contestant">Contestant to register in the tournament.</param>
        public void AddContestant(IContestant contestant);

        /// <summary>
        /// Stores or updates a result entry for a contestant.
        /// </summary>
        /// <param name="contestant">Contestant whose result should be recorded.</param>
        /// <param name="score">Score value to associate with the contestant.</param>
        public void SetResult(IContestant contestant, IScore score);

        /// <summary>
        /// Starts the tournament. 
        /// This method is responsible for initializing the tournament and creating the matches.
        /// </summary>
        public void Start();

        /// <summary>
        /// Finalizes tournament standings and counts final results.
        /// </summary>
        public void End();

        /// <summary>
        /// Advances the tournament by one stage or round.
        /// </summary>
        /// <returns><c>true</c> when advancement created new stage data; otherwise <c>false</c>.</returns>
        public bool Advance();
    }
}
