using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace ModelTests;

// ─── Match ────────────────────────────────────────────────────────────────────

using SportsLibrary.Core;
using SportsLibrary.Football;

[TestFixture]
public class MatchTests
{
    private static IContestant C(string name) => new TeamContestant(name);

    [Test]
    public void Id_UniquePerInstance()
    {
        var m1 = new Match("M", new[] { C("A"), C("B") });
        var m2 = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m1.Id, Is.Not.EqualTo(m2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var m = new Match("Quarter Final", new[] { C("A"), C("B") });
        Assert.That(m.Name, Is.EqualTo("Quarter Final"));
    }

    [Test]
    public void State_ScheduledByDefault()
    {
        var m = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m.State, Is.EqualTo(MatchState.Scheduled));
    }

    [Test]
    public void State_CanBeUpdated()
    {
        var m = new Match("M", new[] { C("A") });
        m.Start();
        Assert.That(m.State, Is.EqualTo(MatchState.InProgress));
    }

    [Test]
    public void Contestants_PopulatedFromConstructor()
    {
        var a = C("A"); var b = C("B");
        var m = new Match("M", new[] { a, b });
        Assert.That(m.Contestants, Is.EquivalentTo(new[] { a, b }));
    }

    [Test]
    public void Statistics_EmptyByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Statistics, Is.Empty);
    }

    [Test]
    public void Statistics_CanStoreScorePerContestant()
    {
        var a = C("A");
        var m = new Match("M", new[] { a });
        var score = new FootballMatchScore(goalsScored: 2);
        m.Start();
        m.SetScore(a, score);
        Assert.That(m.Statistics[a], Is.SameAs(score));
    }

    [Test]
    public void SetScore_UnknownContestant_Throws()
    {
        var m = new Match("M", new[] { C("A") });
        m.Start();

        Assert.Throws<ArgumentException>(() => m.SetScore(C("B"), new FootballMatchScore(goalsScored: 1)));
    }

    [Test]
    public void PenaltyWinner_NullByDefault()
    {
        var m = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m.PenaltyWinner, Is.Null);
    }

    [Test]
    public void PenaltyWinner_CanBeAssigned()
    {
        var b = C("B");
        var m = new Match("M", new[] { C("A"), b });
        m.Start();
        m.AssignPenaltyWinner(b);
        Assert.That(m.PenaltyWinner, Is.SameAs(b));
    }

    [Test]
    public void GetDisqualifiedContestants_EmptyByDefault()
    {
        var m = new Match("M", new[] { C("A"), C("B") });

        Assert.That(m.GetDisqualifiedContestants(), Is.Empty);
    }

    [Test]
    public void Disqualify_MarksContestantAsDisqualified()
    {
        var a = C("A");
        var m = new Match("M", new[] { a, C("B") });
        m.Start();

        m.Disqualify(a, "Rule breach");

        Assert.That(m.IsDisqualified(a), Is.True);
        Assert.That(m.GetDisqualifiedContestants(), Does.Contain(a));
    }

    [Test]
    public void Disqualify_UnknownContestant_Throws()
    {
        var m = new Match("M", new[] { C("A") });
        m.Start();

        Assert.Throws<ArgumentException>(() => m.Disqualify(C("X")));
    }

    [Test]
    public void Disqualify_AfterFinish_IsAllowed()
    {
        var a = C("A");
        var m = new Match("M", new[] { a, C("B") });
        m.Start();
        m.Finish();

        Assert.DoesNotThrow(() => m.Disqualify(a, "Post-event review"));
        Assert.That(m.IsDisqualified(a), Is.True);
    }

    [Test]
    public void RecordEvent_BeforeStart_Throws()
    {
        var a = C("A");
        var m = new Match("M", new[] { a });

        Assert.Throws<InvalidOperationException>(() =>
            m.RecordEvent(new FootballGoalPayload { Contestant = a, Minute = 1 }));
    }

    [Test]
    public void RecordEvent_AfterFinish_Throws()
    {
        var a = C("A");
        var m = new Match("M", new[] { a });
        m.Start();
        m.Finish();

        Assert.Throws<InvalidOperationException>(() =>
            m.RecordEvent(new FootballGoalPayload { Contestant = a, Minute = 90 }));
    }

    [Test]
    public void Timeline_NotNullByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Timeline, Is.Not.Null);
    }

    [Test]
    public void Timeline_EmptyByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Timeline.Events, Has.Count.EqualTo(1));
        Assert.That(m.State, Is.EqualTo(MatchState.Scheduled));
    }

    [Test]
    public void Transition_DefaultPolicy_InvalidTransitionThrows()
    {
        var m = new Match("M", new[] { C("A") });

        Assert.Throws<InvalidOperationException>(() => m.Finish());
    }
}