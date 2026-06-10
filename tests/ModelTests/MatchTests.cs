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
        m.State = MatchState.InProgress;
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
        m.SetScore(a, score);
        Assert.That(m.Statistics[a], Is.SameAs(score));
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
        m.AssignPenaltyWinner(b);
        Assert.That(m.PenaltyWinner, Is.SameAs(b));
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
        Assert.That(m.Timeline.Events, Is.Empty);
    }
}