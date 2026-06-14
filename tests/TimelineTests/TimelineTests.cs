using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── Core Timeline / IInGameEvent tests ──────────────────────────────────────

[TestFixture]
public class TimelineTests
{
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
        var match = M();
        var timeline = match.Timeline;
        var ev = new InGameEvent(DateTime.Now, new SkiJumpPayload());

        match.RecordEvent(ev);

        Assert.That(timeline.Events.Any(e => e.Timestamp == ev.Timestamp && e.GetEvent() == ev.GetEvent()), Is.True);
    }

    [Test]
    public void AddEvent_MultipleEvents_AllPreserved()
    {
        var match = M();
        var timeline = match.Timeline;
        var events = Enumerable.Range(0, 5)
            .Select(_ => new InGameEvent(DateTime.Now, new SkiJumpPayload()))
            .ToList<IInGameEvent>();

        foreach (var ev in events) match.RecordEvent(ev);

        Assert.That(timeline.Events.Count, Is.EqualTo(6));
        Assert.That(timeline.Events.Skip(1).Select(e => e.GetEvent()), Is.EquivalentTo(events.Select(e => e.GetEvent())));
    }

    [Test]
    public void Events_IsReadOnly_CannotBeModifiedExternally()
    {
        var timeline = M().Timeline;
        // IReadOnlyList — no Add/Remove exposed
        Assert.That(timeline.Events, Is.InstanceOf<IReadOnlyList<IInGameEvent>>());
    }

    // ── replay order ──────────────────────────────────────────────────────────

    [Test]
    public void RepeatTimeline_ReplayedInChronologicalOrder()
    {
        var match = M();
        var timeline = match.Timeline;
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);

        // Add in reverse order
        match.RecordEvent(new InGameEvent(base_.AddMinutes(30), new SkiJumpPayload()));
        match.RecordEvent(new InGameEvent(base_.AddMinutes(10), new SkiJumpPayload()));
        match.RecordEvent(new InGameEvent(base_.AddMinutes(50), new SkiJumpPayload()));

        var replayed = new List<DateTime>();
        timeline.RepeatTimeline(ev => replayed.Add(ev.Timestamp));

        Assert.That(replayed, Is.Ordered.Ascending);
    }

    [Test]
    public void RepeatTimeline_EmptyTimeline_CallbackNeverInvoked()
    {
        var timeline = new Timeline();
        int calls = 0;

        timeline.RepeatTimeline(_ => calls++);

        Assert.That(calls, Is.EqualTo(0));
    }

    [Test]
    public void RepeatTimeline_PayloadsAccessibleDuringReplay()
    {
        var match = M();
        var timeline = match.Timeline;
        var contestant = C("Kamil");
        var payload = new SkiJumpPayload { Contestant = contestant, Score = Pts(200f) };
        match.RecordEvent(new InGameEvent(DateTime.Now, payload));

        IEventPayload? captured = null;
        timeline.RepeatTimeline(ev => captured = ev.GetEvent());

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