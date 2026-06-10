namespace SportsLibrary.Core
{
    public interface IRankingStrategy
    {
        IReadOnlyDictionary<IContestant, IScore> InitializeScores(IEnumerable<IContestant> contestants);
        IReadOnlyList<(IContestant Contestant, IScore Score)> Rank(IReadOnlyDictionary<IContestant, IScore> scores);
    }
}
