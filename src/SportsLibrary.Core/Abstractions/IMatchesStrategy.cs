namespace SportsLibrary.Core
{
    public interface IMatchesStrategy
    {
        List<IMatch> CreateMatches(List<IContestant> contestants);
        List<IMatch>? CreateNextRound(List<IMatch> completedMatches);
    }
}
