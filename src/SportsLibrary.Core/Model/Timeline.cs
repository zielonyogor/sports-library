namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a timeline of in-game events. Events are stored in the order they were added, additionally identified by their timestamps. 
    /// Provides methods to add events, repeat the timeline, and query events by payload type.
    /// </summary>
    public sealed class Timeline
    {
        private readonly List<IInGameEvent> _events = new();
        private readonly HashSet<ITimelineListener> _listeners = new();

        /// <summary>
        /// List of in-game events. Each event is expected to have a timestamp and a payload describing the event details.
        /// </summary>
        public IReadOnlyList<IInGameEvent> Events => _events;

        /// <summary>
        /// Adds a new in-game event to the timeline.
        /// </summary>
        /// <param name="gameEvent">The in-game event to add.</param>
        internal void AddEvent(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);

            if (_events.Count > 0 && gameEvent.Timestamp < _events[^1].Timestamp)
                throw new InvalidOperationException("Cannot append an event older than the last recorded event.");

            _events.Add(gameEvent);

            foreach (var listener in _listeners)
                listener.OnEventRecorded(gameEvent);
        }

        public void Subscribe(ITimelineListener listener, bool replayExisting = false)
        {
            ArgumentNullException.ThrowIfNull(listener);

            if (!_listeners.Add(listener))
                return;

            if (!replayExisting)
                return;

            foreach (var gameEvent in _events)
                listener.OnEventRecorded(gameEvent);
        }

        public bool Unsubscribe(ITimelineListener listener)
        {
            ArgumentNullException.ThrowIfNull(listener);
            return _listeners.Remove(listener);
        }

        /// <summary>
        /// Retrieves all events with a payload of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the event payload.</typeparam>
        /// <returns>A list of events with the specified payload type.</returns>
        public IReadOnlyList<T> GetEventsByPayloadType<T>() where T : class, IEventPayload =>
            _events
                .Select(e => e.GetEvent() as T)
                .Where(p => p is not null)
                .Select(p => p!)
                .ToList();
    }
}
