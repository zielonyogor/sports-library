public interface IInGameEvent
{
    public DateTime timestamp {get; set;}
    public IEventPayload GetEvent()
}