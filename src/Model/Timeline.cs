namespace SportsLibrary.Model
{
    public class Timeline
    {
        private readonly List<IInGameEvent> _events = new();

        public IReadOnlyList<IInGameEvent> Events => _events;

        public void AddEvent(IInGameEvent gameEvent) => _events.Add(gameEvent);

        public void RepeatTimeline(Action<IInGameEvent> onEvent)
        {
            foreach (var ev in _events.OrderBy(e => e.Timestamp))
                onEvent(ev);
        }
    }
}
