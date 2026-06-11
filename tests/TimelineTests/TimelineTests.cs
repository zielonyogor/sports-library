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

    // ── add & read ────────────────────────────────────────────────────────────

    [Test]
    public void AddEvent_EventAppearsInEvents()
    {
        var timeline = new Timeline();
        var ev = new InGameEvent(DateTime.Now, new SkiJumpPayload());

        timeline.AddEvent(ev);

        Assert.That(timeline.Events, Contains.Item(ev));
    }

    [Test]
    public void AddEvent_MultipleEvents_AllPreserved()
    {
        var timeline = new Timeline();
        var events = Enumerable.Range(0, 5)
            .Select(_ => new InGameEvent(DateTime.Now, new SkiJumpPayload()))
            .ToList<IInGameEvent>();

        foreach (var ev in events) timeline.AddEvent(ev);

        Assert.That(timeline.Events.Count, Is.EqualTo(5));
        Assert.That(timeline.Events, Is.EquivalentTo(events));
    }

    [Test]
    public void Events_IsReadOnly_CannotBeModifiedExternally()
    {
        var timeline = new Timeline();
        // IReadOnlyList — no Add/Remove exposed
        Assert.That(timeline.Events, Is.InstanceOf<IReadOnlyList<IInGameEvent>>());
    }

    // ── replay order ──────────────────────────────────────────────────────────

    [Test]
    public void RepeatTimeline_ReplayedInChronologicalOrder()
    {
        var timeline = new Timeline();
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);

        // Add in reverse order
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(30), new SkiJumpPayload()));
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(10), new SkiJumpPayload()));
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(50), new SkiJumpPayload()));

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
        var timeline = new Timeline();
        var contestant = C("Kamil");
        var payload = new SkiJumpPayload { Contestant = contestant, Score = Pts(200f) };
        timeline.AddEvent(new InGameEvent(DateTime.Now, payload));

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