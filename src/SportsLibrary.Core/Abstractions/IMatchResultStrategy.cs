namespace SportsLibrary.Core
{
    public interface IMatchResultStrategy
    {
        IContestant? DetermineWinner(IMatch match);
    }
}
