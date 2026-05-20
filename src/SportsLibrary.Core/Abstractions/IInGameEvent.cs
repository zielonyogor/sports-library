namespace SportsLibrary.Core
{
    public interface IInGameEvent
    {
        DateTime Timestamp { get; set; }
        IEventPayload GetEvent();
    }
}
