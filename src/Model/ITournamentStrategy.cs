namespace SportsLibrary.Model
{
    public interface ITournamentStrategy
    {
        List<ITournament> CreateSubTournaments(List<IContestant> contestants);
        List<ITournament>? CreateNextStage(List<ITournament> completedTournaments);
        Dictionary<IContestant, IScore> AggregateResults(List<ITournament> tournaments);
    }
}
