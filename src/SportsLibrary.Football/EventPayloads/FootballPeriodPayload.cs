using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    public class FootballPeriodPayload : IEventPayload
    {
        public MatchPeriod Period { get; init; }
    }
}
