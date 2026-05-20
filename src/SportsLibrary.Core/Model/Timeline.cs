namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a timeline of in-game events. Events are stored in the order they were added, additionally identified by their timestamps. 
    /// Provides methods to add events, repeat the timeline, and query events by payload type.
    /// </summary>
    public class Timeline
    {
        private readonly List<IInGameEvent> _events = new();

        /// <summary>
        /// List of in-game events. Each event is expected to have a timestamp and a payload describing the event details.
        /// </summary>
        public IReadOnlyList<IInGameEvent> Events => _events;

        /// <summary>
        /// Adds a new in-game event to the timeline.
        /// </summary>
        /// <param name="gameEvent">The in-game event to add.</param>
        public void AddEvent(IInGameEvent gameEvent)
        {
            ArgumentNullException.ThrowIfNull(gameEvent);
            _events.Add(gameEvent);
        }

        /// <summary>
        /// Repeats the timeline, invoking the specified action for each event in chronological order.
        /// </summary>
        /// <param name="onEvent">The action to invoke for each event.</param>
        public void RepeatTimeline(Action<IInGameEvent> onEvent)
        {
            foreach (var ev in _events.OrderBy(e => e.Timestamp))
                onEvent(ev);
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
