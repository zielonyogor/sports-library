namespace SportsLibrary.Core
{
    public class Timeline
    {
        private readonly List<IInGameEvent> _events = new();

        public IReadOnlyList<IInGameEvent> Events => _events;

        public void AddEvent(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            _events.Add(gameEvent);
        }

        public void RepeatTimeline(Action<IInGameEvent> onEvent)
        {
            foreach (var ev in _events.OrderBy(e => e.Timestamp))
                onEvent(ev);
        }

        public IReadOnlyList<T> GetEventsByPayloadType<T>() where T : class, IEventPayload =>
            _events
                .Select(e => e.GetEvent() as T)
                .Where(p => p is not null)
                .Select(p => p!)
                .ToList();

        public bool RemoveLastEvent()
        {
            if (_events.Count == 0) return false;
            _events.RemoveAt(_events.Count - 1);
            return true;
        }
    }
}
