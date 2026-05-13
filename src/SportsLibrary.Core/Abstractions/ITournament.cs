namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing a tournament. A tournament is a collection of matches that are played between contestants. 
    /// The tournament is responsible for managing the matches, tracking the contestants and determining the winner of the tournament.
    /// </summary>
    public interface ITournament
    {
        public Guid Id { get; }
        public String Name { get; set; }
        public List<IContestant> Contestants { get; set; }
        public Dictionary<IContestant, IScore> TournamentResults { get; set; }

        public void Start();
        public void End();
    }
}