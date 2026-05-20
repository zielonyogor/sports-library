namespace SportsLibrary.Core
{
    public interface ITournamentStrategy
    {
        List<ITournament> CreateSubTournaments(List<IContestant> contestants);
        List<ITournament>? CreateNextStage(List<ITournament> completedTournaments);
        Dictionary<IContestant, IScore> AggregateResults(List<ITournament> tournaments);
    }
}
