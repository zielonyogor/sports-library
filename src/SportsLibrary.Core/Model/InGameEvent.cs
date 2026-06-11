namespace SportsLibrary.Core
{
    public sealed class InGameEvent : IInGameEvent
    {
        public DateTime Timestamp { get; }
        public IEventPayload Payload { get; }

        public InGameEvent(DateTime timestamp, IEventPayload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            Timestamp = timestamp;
            Payload = payload;
        }

        public IEventPayload GetEvent() => Payload;
    }
}
