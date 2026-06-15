using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── Core Timeline / IInGameEvent tests ──────────────────────────────────────

[TestFixture]
public class TimelineTests
{
    private sealed class RecordingListener : ITimelineListener
    {
        public readonly List<IInGameEvent> Events = new();

        public void OnEventRecorded(IInGameEvent gameEvent)
        {
            Events.Add(gameEvent);
        }
    }

    private static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    private static IScore Pts(float v) => new SkiJumpingScore(v, 0f, 0f, 0f);

    private static MatchSupervisor Referee() =>
        new MatchSupervisor(new Person("Referee", "Smith"));

    private static Match M() => new Match("Test", new[] { C("A"), C("B") });

    // ── add & read ────────────────────────────────────────────────────────────

    [Test]
    public void AddEvent_EventAppearsInEvents()
    {
        var baseTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var match = new Match("Test", new[] { C("A"), C("B") }, baseTime.AddMinutes(-2));
        match.Start(baseTime.AddMinutes(-1));
        var timeline = match.Timeline;
        var ev = new InGameEvent(baseTime, new SkiJumpPayload());

        match.RecordEvent(ev);

        Assert.That(timeline.Events.Any(e => e.Timestamp == ev.Timestamp && e.GetEvent() == ev.GetEvent()), Is.True);
    }

    [Test]
    public void AddEvent_MultipleEvents_AllPreserved()
    {
        var baseTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var match = new Match("Test", new[] { C("A"), C("B") }, baseTime.AddMinutes(-2));
        match.Start(baseTime.AddMinutes(-1));
        var timeline = match.Timeline;
        var events = Enumerable.Range(0, 5)
            .Select(i => new InGameEvent(baseTime.AddMinutes(i), new SkiJumpPayload()))
            .ToList<IInGameEvent>();

        foreach (var ev in events) match.RecordEvent(ev);

        Assert.That(timeline.Events.Count, Is.EqualTo(7));
        Assert.That(timeline.Events.Skip(2).Select(e => e.GetEvent()), Is.EquivalentTo(events.Select(e => e.GetEvent())));
    }

    [Test]
    public void Events_IsReadOnly_CannotBeModifiedExternally()
    {
        var timeline = M().Timeline;
        // IReadOnlyList — no Add/Remove exposed
        Assert.That(timeline.Events, Is.InstanceOf<IReadOnlyList<IInGameEvent>>());
    }

    // ── observer behavior ─────────────────────────────────────────────────────

    [Test]
    public void AddEvent_OutOfOrderTimestamp_Throws()
    {
        var match = M();
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);

        var orderedMatch = new Match("Test", new[] { C("A"), C("B") }, base_);
        orderedMatch.Start(base_.AddMinutes(1));
        orderedMatch.RecordEvent(new InGameEvent(base_.AddMinutes(30), new SkiJumpPayload()));

        Assert.Throws<InvalidOperationException>(() =>
            orderedMatch.RecordEvent(new InGameEvent(base_.AddMinutes(10), new SkiJumpPayload())));
    }

    [Test]
    public void Subscribe_ListenerCalledInRecordOrder()
    {
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);
        var match = new Match("Test", new[] { C("A"), C("B") }, base_);
        match.Start(base_.AddMinutes(1));

        var listener = new RecordingListener();
        match.Timeline.Subscribe(listener);

        var ev1 = new InGameEvent(base_.AddMinutes(10), new SkiJumpPayload());
        var ev2 = new InGameEvent(base_.AddMinutes(20), new SkiJumpPayload());
        var ev3 = new InGameEvent(base_.AddMinutes(30), new SkiJumpPayload());

        match.RecordEvent(ev1);
        match.RecordEvent(ev2);
        match.RecordEvent(ev3);

        Assert.That(listener.Events.Select(e => e.Timestamp), Is.EqualTo(new[] { ev1.Timestamp, ev2.Timestamp, ev3.Timestamp }));
    }

    [Test]
    public void Subscribe_DuplicateListener_NotRegisteredTwice()
    {
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);
        var match = new Match("Test", new[] { C("A"), C("B") }, base_);
        match.Start(base_.AddMinutes(1));
        var listener = new RecordingListener();

        match.Timeline.Subscribe(listener);
        match.Timeline.Subscribe(listener);

        match.RecordEvent(new InGameEvent(base_.AddMinutes(2), new SkiJumpPayload()));

        Assert.That(listener.Events, Has.Count.EqualTo(1));
    }

    [Test]
    public void Unsubscribe_RemovesListener()
    {
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);
        var match = new Match("Test", new[] { C("A"), C("B") }, base_);
        match.Start(base_.AddMinutes(1));
        var listener = new RecordingListener();

        match.Timeline.Subscribe(listener);
        match.RecordEvent(new InGameEvent(base_.AddMinutes(2), new SkiJumpPayload()));
        var removed = match.Timeline.Unsubscribe(listener);
        match.RecordEvent(new InGameEvent(base_.AddMinutes(3), new SkiJumpPayload()));

        Assert.That(removed, Is.True);
        Assert.That(listener.Events, Has.Count.EqualTo(1));
    }

    [Test]
    public void Listener_NotCalledWhenValidationFails()
    {
        var contestant = C("A");
        var match = new Match("Test", new[] { contestant, C("B") });
        var listener = new RecordingListener();
        match.Timeline.Subscribe(listener);

        Assert.Throws<InvalidOperationException>(() =>
            match.RecordEvent(new FootballGoalPayload { Contestant = contestant, Minute = 10 }));

        Assert.That(listener.Events, Is.Empty);
    }

    [Test]
    public void Listener_PayloadAccessibleAfterEventIsRecorded()
    {
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);
        var contestant = C("Kamil");
        var match = new Match("Test", new[] { contestant, C("B") }, base_);
        match.Start(base_.AddMinutes(1));

        var listener = new RecordingListener();
        match.Timeline.Subscribe(listener);

        var payload = new SkiJumpPayload { Contestant = contestant, Score = Pts(200f) };
        match.RecordEvent(new InGameEvent(base_.AddMinutes(2), payload));

        IEventPayload? captured = listener.Events.Single().GetEvent();

        Assert.That(captured, Is.SameAs(payload));
        var ep = (SkiJumpPayload)captured!;
        Assert.That(ep.Contestant, Is.SameAs(contestant));
        Assert.That(ep.Score!.GetValue(), Is.EqualTo(200).Within(0.01));
    }

    // ── SkiJumpPayload properties ───────────────────────────────────────────────

    [Test]
    public void SkiJumpPayload_AllPropertiesNullByDefault()
    {
        var payload = new SkiJumpPayload();

        Assert.That(payload.Score, Is.Null);
        Assert.That(payload.Contestant, Is.Null);
        Assert.That(payload.Referee, Is.Null);
    }

    [Test]
    public void SkiJumpPayload_PropertiesSetViaInitializer()
    {
        var contestant = C("Stefan");
        var referee = Referee();
        var score = Pts(150f);

        var payload = new SkiJumpPayload
        {
            Contestant = contestant,
            Referee = referee,
            Score = score,
        };

        Assert.That(payload.Contestant, Is.SameAs(contestant));
        Assert.That(payload.Referee, Is.SameAs(referee));
        Assert.That(payload.Score, Is.SameAs(score));
    }
}