using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

// ─── SkiJumpMatchController ───────────────────────────────────────────────────

[TestFixture]
public class SkiJumpMatchControllerTests
{
    private static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    private static MatchSupervisor R(string name) => new MatchSupervisor(new Person(name, ""));

    [Test]
    public void GetTotalScore_SumsAllJumpsForContestant()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var t = DateTime.Now;
        match.Timeline.AddEvent(new InGameEvent(t, new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) }));  // 187
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(135f, 58f, 1f, 0f) })); // 194

        Assert.That(new SkiJumpMatchController(match).GetTotalScore(kamil), Is.EqualTo(381).Within(0.01));
    }

    [Test]
    public void GetBestJump_ReturnsMaximumJumpScore()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var t = DateTime.Now;
        match.Timeline.AddEvent(new InGameEvent(t, new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) })); // 187
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(135f, 58f, 1f, 0f) })); // 194

        Assert.That(new SkiJumpMatchController(match).GetBestJump(kamil), Is.EqualTo(194).Within(0.01));
    }

    [Test]
    public void GetBestJump_NoJumps_ReturnsZero()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        Assert.That(new SkiJumpMatchController(match).GetBestJump(kamil), Is.EqualTo(0));
    }

    [Test]
    public void IsDisqualified_WhenDisqualificationPayloadPresent_ReturnsTrue()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        match.Timeline.AddEvent(new InGameEvent(DateTime.Now,
            new SkiJumpingDisqualificationPayload { Contestant = kamil, Reason = "Suit violation" }));

        Assert.That(new SkiJumpMatchController(match).IsDisqualified(kamil), Is.True);
    }

    [Test]
    public void IsDisqualified_WhenAbsent_ReturnsFalse()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        Assert.That(new SkiJumpMatchController(match).IsDisqualified(kamil), Is.False);
    }

    [Test]
    public void IsDisqualified_OtherPlayerDisqualified_ReturnsFalseForCleanJumper()
    {
        var kamil = C("Kamil");
        var dawid = C("Dawid");
        var match = new Match("Finals", new[] { kamil, dawid });
        match.Timeline.AddEvent(new InGameEvent(DateTime.Now,
            new SkiJumpingDisqualificationPayload { Contestant = dawid, Reason = "Gate infringement" }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.IsDisqualified(kamil), Is.False);
        Assert.That(ctrl.IsDisqualified(dawid), Is.True);
    }
}