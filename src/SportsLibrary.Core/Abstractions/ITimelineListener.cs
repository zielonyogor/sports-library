namespace SportsLibrary.Core
{
    /// <summary>
    /// Observer contract for classes that react to events appended to a <see cref="Timeline"/>.
    /// </summary>
    public interface ITimelineListener
    {
        void OnEventRecorded(IInGameEvent gameEvent);
    }
}