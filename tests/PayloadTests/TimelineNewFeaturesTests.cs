using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

// ─── Timeline: GetEventsByPayloadType + null validation ──────

[TestFixture]
public class TimelineNewFeaturesTests
{
    private static IContestant C(string name) => new TeamContestant(name);
    private static MatchSupervisor R(string name) => new MatchSupervisor(new Person(name, ""));
    private static Match M(params IContestant[] contestants) => new Match("Test", contestants);

    [Test]
    public void GetEventsByPayloadType_ReturnsOnlyMatchingPayloads()
    {
        var match = M(C("Red"), C("Blue"));
        var t = DateTime.Now;
        var red = C("Red");
        var referee = R("Referee");
        match.RecordEvent(new FootballGoalPayload { Contestant = red, Minute = 10 }, t);
        match.RecordEvent(new FootballCardPayload { Contestant = red, CardType = CardType.Yellow, Minute = 15, Referee = referee }, t.AddMinutes(5));
        match.RecordEvent(new FootballGoalPayload { Contestant = red, Minute = 20 }, t.AddMinutes(10));

        var goals = match.Timeline.GetEventsByPayloadType<FootballGoalPayload>();

        Assert.That(goals.Count, Is.EqualTo(2));
        Assert.That(goals.Select(g => g.Minute), Is.EquivalentTo(new[] { 10, 20 }));
    }

    [Test]
    public void GetEventsByPayloadType_NoMatchingPayloads_ReturnsEmpty()
    {
        var match = M(C("Red"));
        var referee = R("Referee");
        match.RecordEvent(new FootballCardPayload { Contestant = C("Red"), CardType = CardType.Yellow, Minute = 15, Referee = referee }, DateTime.Now);

        var goals = match.Timeline.GetEventsByPayloadType<FootballGoalPayload>();

        Assert.That(goals, Is.Empty);
    }

    [Test]
    public void GetEventsByPayloadType_EmptyTimeline_ReturnsEmpty()
    {
        var match = M(C("Red"));
        Assert.That(match.Timeline.GetEventsByPayloadType<FootballGoalPayload>(), Is.Empty);
    }

    [Test]
    public void AddEvent_NullEvent_Throws()
    {
        var match = M(C("Red"));
        Assert.Throws<ArgumentNullException>(() => match.RecordEvent((IInGameEvent)null!));
    }

    [Test]
    public void InGameEvent_NullPayload_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InGameEvent(DateTime.Now, null!));
    }
}