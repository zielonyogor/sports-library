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
        public IReadOnlyList<IContestant> Contestants { get; }
        public IReadOnlyDictionary<IContestant, IScore> TournamentResults { get; }

        public void AddContestant(IContestant contestant);
        public void SetResult(IContestant contestant, IScore score);

        public void Start();
        public void End();
    }
}
