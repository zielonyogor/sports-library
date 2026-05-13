namespace SportsLibrary.Core
{
    public interface IRankingStrategy
    {
        Dictionary<IContestant, IScore> InitializeScores(IEnumerable<IContestant> contestants);
        List<(IContestant Contestant, IScore Score)> Rank(Dictionary<IContestant, IScore> scores);
    }
}
