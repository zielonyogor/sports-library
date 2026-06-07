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

    [Test]
    public void GetEventsByPayloadType_ReturnsOnlyMatchingPayloads()
    {
        var timeline = new Timeline();
        var t = DateTime.Now;
        var red = C("Red");
        var referee = R("Referee");
        timeline.AddEvent(new InGameEvent(t, new FootballGoalPayload { Contestant = red, Minute = 10 }));
        timeline.AddEvent(new InGameEvent(t.AddMinutes(5), new FootballCardPayload { Contestant = red, CardType = CardType.Yellow, Minute = 15, Referee = referee }));
        timeline.AddEvent(new InGameEvent(t.AddMinutes(10), new FootballGoalPayload { Contestant = red, Minute = 20 }));

        var goals = timeline.GetEventsByPayloadType<FootballGoalPayload>();

        Assert.That(goals.Count, Is.EqualTo(2));
        Assert.That(goals.Select(g => g.Minute), Is.EquivalentTo(new[] { 10, 20 }));
    }

    [Test]
    public void GetEventsByPayloadType_NoMatchingPayloads_ReturnsEmpty()
    {
        var timeline = new Timeline();
        var referee = R("Referee");
        timeline.AddEvent(new InGameEvent(DateTime.Now, new FootballCardPayload { Contestant = C("Red"), CardType = CardType.Yellow, Minute = 15, Referee = referee }));

        var goals = timeline.GetEventsByPayloadType<FootballGoalPayload>();

        Assert.That(goals, Is.Empty);
    }

    [Test]
    public void GetEventsByPayloadType_EmptyTimeline_ReturnsEmpty()
    {
        var timeline = new Timeline();
        Assert.That(timeline.GetEventsByPayloadType<FootballGoalPayload>(), Is.Empty);
    }

    [Test]
    public void AddEvent_NullEvent_Throws()
    {
        var timeline = new Timeline();
        Assert.Throws<ArgumentNullException>(() => timeline.AddEvent(null!));
    }

    [Test]
    public void InGameEvent_NullPayload_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InGameEvent(DateTime.Now, null!));
    }
}