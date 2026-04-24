namespace SportsLibrary.Model
{
    public interface IMatchResultStrategy
    {
        IContestant? DetermineWinner(IMatch match);
    }
}
