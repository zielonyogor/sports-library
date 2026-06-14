namespace SportsLibrary.Core
{
    public sealed class MatchStatisticsTracker : ITimelineListener
    {
        private readonly Dictionary<IContestant, IScore> _statistics = new();

        public IReadOnlyDictionary<IContestant, IScore> Statistics => _statistics;

        public IScore? GetCurrentScore(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            return _statistics.GetValueOrDefault(contestant);
        }

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            if (gameEvent.GetEvent() is IScoreEventPayload payload && payload.Contestant is { } contestant)
                _statistics[contestant] = payload.Apply(_statistics.GetValueOrDefault(contestant));
        }
    }
}