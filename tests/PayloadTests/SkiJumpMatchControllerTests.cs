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
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) }));  // 187
        match.RecordEvent(new InGameEvent(t.AddMinutes(60), new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(135f, 58f, 1f, 0f) })); // 194

        Assert.That(new SkiJumpMatchController(match).GetTotalScore(kamil), Is.EqualTo(381).Within(0.01));
    }

    [Test]
    public void GetBestJump_ReturnsMaximumJumpScore()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) })); // 187
        match.RecordEvent(new InGameEvent(t.AddMinutes(60), new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(135f, 58f, 1f, 0f) })); // 194

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
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t,
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
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil, dawid }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t,
            new SkiJumpingDisqualificationPayload { Contestant = dawid, Reason = "Gate infringement" }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.IsDisqualified(kamil), Is.False);
        Assert.That(ctrl.IsDisqualified(dawid), Is.True);
    }

    [Test]
    public void GetCurrentGateCompensation_NoGateChange_ReturnsZero()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 2f) }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.GetCurrentGateCompensation(), Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void GetCurrentGateCompensation_AfterGateLowered_ReturnsCompensationValue()
    {
        var kamil = C("Kamil");
        var judge = R("Judge");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 12, GatesLowered = 2, CompensationPerJump = 7.2f, Referee = judge }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.GetCurrentGateCompensation(), Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void GetCurrentGateCompensation_MultipleGateChanges_ReturnsLastCompensation()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(5), new GateLoweredPayload { NewGate = 11, GatesLowered = 2, CompensationPerJump = 7.2f }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.GetCurrentGateCompensation(), Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void GetCurrentGateCompensation_AfterGateHigher_ReturnsNegativeCompensation()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateHigherPayload { NewGate = 14, GatesRaised = 2, CompensationPerJump = -7.2f }));

        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.GetCurrentGateCompensation(), Is.EqualTo(-7.2f).Within(0.001f));
    }

    [Test]
    public void GetGateChangeHistory_NoChanges_ReturnsEmptyList()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var ctrl = new SkiJumpMatchController(match);
        Assert.That(ctrl.GetGateChangeHistory(), Is.Empty);
    }

    [Test]
    public void GetGateChangeHistory_WithChanges_ReturnsAllChanges()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(5), new GateLoweredPayload { NewGate = 11, GatesLowered = 2, CompensationPerJump = 7.2f }));

        var ctrl = new SkiJumpMatchController(match);
        var history = ctrl.GetGateChangeHistory();
        Assert.That(history, Has.Count.EqualTo(2));
        Assert.That(history[0].CompensationPerJump, Is.EqualTo(3.6f).Within(0.001f));
        Assert.That(history[1].CompensationPerJump, Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void GetGateChangeHistory_WithLowerAndHigherPayloads_ReturnsBoth()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(3), new GateHigherPayload { NewGate = 14, GatesRaised = 1, CompensationPerJump = -3.6f }));

        var ctrl = new SkiJumpMatchController(match);
        var history = ctrl.GetGateChangeHistory();

        Assert.That(history, Has.Count.EqualTo(2));
        Assert.That(history[0], Is.TypeOf<GateLoweredPayload>());
        Assert.That(history[1], Is.TypeOf<GateHigherPayload>());
        Assert.That(history[1].CompensationPerJump, Is.EqualTo(-3.6f).Within(0.001f));
    }

    [Test]
    public void CreateScoreWithCurrentGateCompensation_NoGateChange_UsesZeroCompensation()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var ctrl = new SkiJumpMatchController(match);

        var score = ctrl.CreateScoreWithCurrentGateCompensation(130f, 57f, 0f);

        Assert.That(score.DistancePoints, Is.EqualTo(130f));
        Assert.That(score.StylePoints, Is.EqualTo(57f));
        Assert.That(score.WindCompensation, Is.EqualTo(0f));
        Assert.That(score.GateCompensation, Is.EqualTo(0f));
        Assert.That(score.GetValue(), Is.EqualTo(187).Within(0.01));
    }

    [Test]
    public void CreateScoreWithCurrentGateCompensation_WithGateChange_AutomaticallyAppliesCompensation()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 12, GatesLowered = 2, CompensationPerJump = 7.2f }));

        var ctrl = new SkiJumpMatchController(match);
        var score = ctrl.CreateScoreWithCurrentGateCompensation(130f, 57f, 1.5f);

        Assert.That(score.DistancePoints, Is.EqualTo(130f));
        Assert.That(score.StylePoints, Is.EqualTo(57f));
        Assert.That(score.WindCompensation, Is.EqualTo(1.5f));
        Assert.That(score.GateCompensation, Is.EqualTo(7.2f).Within(0.001f));
        Assert.That(score.GetValue(), Is.EqualTo(195.7f).Within(0.01));
    }

    [Test]
    public void CreateScoreWithCurrentGateCompensation_AfterMultipleGateChanges_AppliesLastCompensation()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(5), new GateLoweredPayload { NewGate = 11, GatesLowered = 2, CompensationPerJump = 7.2f }));

        var ctrl = new SkiJumpMatchController(match);
        var score = ctrl.CreateScoreWithCurrentGateCompensation(135f, 58f, 0f);

        Assert.That(score.GateCompensation, Is.EqualTo(7.2f).Within(0.001f));
        Assert.That(score.GetValue(), Is.EqualTo(200.2f).Within(0.01));
    }

    [Test]
    public void CreateJumpPayload_NoGateChange_UsesZeroGateCompensation()
    {
        var kamil = C("Kamil");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));

        var ctrl = new SkiJumpMatchController(match);
        var payload = ctrl.CreateJumpPayload(kamil, 130f, 130f, 57f, 0f);

        Assert.That(payload.Contestant, Is.SameAs(kamil));
        Assert.That(payload.Distance, Is.EqualTo(130f));
        Assert.That(payload.Score, Is.TypeOf<SkiJumpingScore>());
        var score = (SkiJumpingScore)payload.Score!;
        Assert.That(score.GateCompensation, Is.EqualTo(0f).Within(0.001f));
        Assert.That(score.GetValue(), Is.EqualTo(187).Within(0.01));
    }

    [Test]
    public void CreateJumpPayload_AfterGateLowered_InjectsCurrentGateCompensation()
    {
        var kamil = C("Kamil");
        var judge = R("Judge");
        var t = DateTime.Now;
        var match = new Match("Finals", new[] { kamil }, t.AddMinutes(-2));
        match.Start(t.AddMinutes(-1));
        match.RecordEvent(new InGameEvent(t, new GateLoweredPayload
        {
            NewGate = 12,
            GatesLowered = 2,
            CompensationPerJump = 7.2f,
            Referee = judge,
        }));

        var ctrl = new SkiJumpMatchController(match);
        var payload = ctrl.CreateJumpPayload(kamil, 130f, 130f, 57f, 1.5f, judge);

        Assert.That(payload.Referee, Is.SameAs(judge));
        var score = payload.Score as SkiJumpingScore;
        Assert.That(score, Is.Not.Null);
        Assert.That(score!.GateCompensation, Is.EqualTo(7.2f).Within(0.001f));
        Assert.That(score.GetValue(), Is.EqualTo(195.7f).Within(0.01));
    }

    [Test]
    public void CreateJumpPayload_WithNullContestant_Throws()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var ctrl = new SkiJumpMatchController(match);

        Assert.That(() => ctrl.CreateJumpPayload(null!, 130f, 130f, 57f), Throws.ArgumentNullException);
    }
}