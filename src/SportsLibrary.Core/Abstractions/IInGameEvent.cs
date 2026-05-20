namespace SportsLibrary.Core
{
    /// <summary>
    /// Interface representing an in-game event. 
    /// An in-game event is an occurrence that happens during a match, such as a goal, a foul, or a substitution.
    /// </summary>
    public interface IInGameEvent
    {
        DateTime Timestamp { get; set; }
        IEventPayload GetEvent();
    }
}
