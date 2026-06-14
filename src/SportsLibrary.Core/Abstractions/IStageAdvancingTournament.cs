namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a tournament that can advance through one or more stages.
    /// </summary>
    public interface IStageAdvancingTournament
    {
        bool Advance();
    }
}