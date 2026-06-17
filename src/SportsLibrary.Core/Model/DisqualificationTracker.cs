namespace SportsLibrary.Core
{
    public sealed class DisqualificationTracker : ITimelineListener
    {
        private readonly HashSet<IContestant> _disqualified = new();

        public bool IsDisqualified(IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(contestant);
            return _disqualified.Contains(contestant);
        }

        public IReadOnlyCollection<IContestant> GetDisqualifiedContestants() =>
            _disqualified.ToList();

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);

            if (gameEvent.GetEvent() is IDisqualificationEventPayload { Contestant: not null } payload)
                _disqualified.Add(payload.Contestant);
        }
    }
}