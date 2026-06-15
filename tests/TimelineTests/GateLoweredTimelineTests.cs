using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── Ski jumping gate-change timeline tests ───────────────────────────────────

[TestFixture]
public class GateLoweredTimelineTests
{
    private static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    private static MatchSupervisor GateJudge() =>
        new MatchSupervisor(new Person("Gate", "Judge"));

    // ── GateLoweredPayload unit ───────────────────────────────────────────────

    [Test]
    public void GateLoweredPayload_ContestantIsNullByDefault()
    {
        var payload = new GateLoweredPayload
        {
            NewGate = 12,
            GatesLowered = 2,
            CompensationPerJump = 7.2f,
        };

        Assert.That(payload.Contestant, Is.Null);
    }

    [Test]
    public void GateLoweredPayload_ExposesGateDataAndReferee()
    {
        var judge = GateJudge();
        var payload = new GateLoweredPayload
        {
            NewGate = 10,
            GatesLowered = 3,
            CompensationPerJump = 10.8f,
            Referee = judge,
        };

        Assert.That(payload.NewGate, Is.EqualTo(10));
        Assert.That(payload.GatesLowered, Is.EqualTo(3));
        Assert.That(payload.CompensationPerJump, Is.EqualTo(10.8f).Within(0.001f));
        Assert.That(payload.Referee, Is.SameAs(judge));
    }

    // ── mixed-event timeline scenarios ───────────────────────────────────────

    [Test]
    public void Timeline_GateChangeAndJumps_CompensationAppliedToSubsequentJumpers()
    {
        // Match: gate at 14; Kamil jumps; gate lowered to 12 (-2); Dawid and Stefan jump.
        // Jumpers after the gate change receive +7.2 pts compensation.
        var kamil = C("Kamil");
        var dawid = C("Dawid");
        var stefan = C("Stefan");
        var judge = GateJudge();
        var t = new DateTime(2024, 1, 28, 10, 0, 0);
        var match = new Match("Qualification", new[] { kamil, dawid, stefan }, t.AddMinutes(-1));
        match.Start(t);

        match.RecordEvent(new InGameEvent(t.AddMinutes(0), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(130f, 56f, -1f, 0f),   // 185 pts, gate 14
            Distance = 130f,
        }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(5), new GateLoweredPayload
        {
            NewGate = 12,
            GatesLowered = 2,
            CompensationPerJump = 7.2f,
            Referee = judge,
        }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(10), new SkiJumpPayload
        {
            Contestant = dawid,
            Score = new SkiJumpingScore(127f, 55.5f, 0.5f, 7.2f),  // base + compensation
            Distance = 127f,
        }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(15), new SkiJumpPayload
        {
            Contestant = stefan,
            Score = new SkiJumpingScore(128f, 55f, -2f, 7.2f),
            Distance = 128f,
        }));

        // Replay: track current gate compensation and accumulate each jumper's final score
        float activeCompensation = 0f;
        var finalScores = new Dictionary<IContestant, double>();
        foreach (var ev in match.Timeline.Events)
        {
            switch (ev.GetEvent())
            {
                case GateLoweredPayload gate:
                    activeCompensation = gate.CompensationPerJump;
                    break;
                case SkiJumpPayload jump when jump.Contestant != null:
                    finalScores[jump.Contestant] = jump.Score?.GetValue() ?? 0;
                    break;
            }
            }

        // Kamil jumped before the gate change → no gate compensation baked in
        Assert.That(finalScores[kamil], Is.EqualTo(185).Within(0.01));
        // Dawid and Stefan jumped after → their scores include +7.2 compensation
        Assert.That(finalScores[dawid], Is.EqualTo(190.2).Within(0.01));
        Assert.That(finalScores[stefan], Is.EqualTo(188.2).Within(0.01));
        // Compensation was captured during replay
        Assert.That(activeCompensation, Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void Timeline_MultipleGateChanges_LastChangeWins()
    {
        var kamil = C("Kamil");
        var t = new DateTime(2024, 1, 28, 10, 0, 0);
        var match = new Match("Final", new[] { kamil }, t.AddMinutes(-1));
        match.Start(t);

        match.RecordEvent(new InGameEvent(t.AddMinutes(5),
            new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(10),
            new GateLoweredPayload { NewGate = 11, GatesLowered = 2, CompensationPerJump = 7.2f }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(15), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(125f, 55f, 0f, 7.2f),  // second change applies
            Distance = 125f,
        }));

        float compensation = 0f;
        foreach (var ev in match.Timeline.Events)
        {
            if (ev.GetEvent() is GateLoweredPayload gate)
                compensation = gate.CompensationPerJump;
        }

        Assert.That(compensation, Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void Timeline_EventTypeDistribution_CorrectCounts()
    {
        var t = new DateTime(2024, 1, 28, 10, 0, 0);
        var a = C("A");
        var b = C("B");
        var match = new Match("Competition", new[] { a, b }, t.AddMinutes(-1));
        match.Start(t);

        // 3 jumps, 2 gate changes
        match.RecordEvent(new InGameEvent(t.AddMinutes(0), new SkiJumpPayload { Contestant = a }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(3), new GateLoweredPayload { NewGate = 13, GatesLowered = 1 }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(6), new SkiJumpPayload { Contestant = b }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(9), new GateLoweredPayload { NewGate = 11, GatesLowered = 2 }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(12), new SkiJumpPayload { Contestant = a }));

        int jumps = 0, gateChanges = 0;
        foreach (var ev in match.Timeline.Events)
        {
            switch (ev.GetEvent())
            {
                case SkiJumpPayload: jumps++; break;
                case GateLoweredPayload: gateChanges++; break;
            }
        }

        Assert.That(jumps, Is.EqualTo(3));
        Assert.That(gateChanges, Is.EqualTo(2));
    }
}
