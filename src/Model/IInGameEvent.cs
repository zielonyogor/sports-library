namespace SportsLibrary.Model
{
    public interface IInGameEvent
    {
        DateTime Timestamp { get; set; }
        IEventPayload GetEvent();
    }
}
