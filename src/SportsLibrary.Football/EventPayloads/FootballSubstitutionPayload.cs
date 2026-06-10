using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballSubstitutionPayload : IEventPayload
    {
        /// <summary>The player coming on.</summary>
        public IContestant? PlayerIn { get; init; }
        /// <summary>The player going off.</summary>
        public IContestant? PlayerOff { get; init; }
        /// <summary>Match minute when the substitution occurred.</summary>
        public int Minute { get; init; }
    }
}
