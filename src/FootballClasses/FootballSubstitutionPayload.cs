using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballSubstitutionPayload : IEventPayload
    {
        /// <summary>The player coming on.</summary>
        public IContestant? Contestant { get; init; }
        /// <summary>The player going off.</summary>
        public IContestant? PlayerOff { get; init; }
        /// <summary>Match minute when the substitution occurred.</summary>
        public int Minute { get; init; }
    }
}
