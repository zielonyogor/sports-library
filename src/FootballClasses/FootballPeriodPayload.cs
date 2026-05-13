using SportsLibrary.Model;

namespace SportsLibrary.FootballClasses
{
    public class FootballPeriodPayload : IEventPayload
    {
        public MatchPeriod Period { get; init; }
    }
}
