namespace SportsLibrary.Core
{
    public interface ITournamentStrategy
    {
        IReadOnlyList<ITournament> CreateSubTournaments(IReadOnlyList<IContestant> contestants);
        IReadOnlyList<ITournament>? CreateNextStage(IReadOnlyList<ITournament> completedTournaments);
        IReadOnlyDictionary<IContestant, IScore> AggregateResults(IReadOnlyList<ITournament> tournaments);
    }
}
