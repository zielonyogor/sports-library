using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public sealed class FootballPeriodPayload : IEventPayload
    {
        public MatchPeriod Period { get; init; }
    }
}
