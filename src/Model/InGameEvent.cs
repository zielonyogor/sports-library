namespace SportsLibrary.Model
{
    public class InGameEvent : IInGameEvent
    {
        public DateTime Timestamp { get; set; }
        public IEventPayload Payload { get; }

        public InGameEvent(DateTime timestamp, IEventPayload payload)
        {
            Timestamp = timestamp;
            Payload = payload;
        }

        public IEventPayload GetEvent() => Payload;
    }
}
