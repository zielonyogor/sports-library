using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── SkiJumpPayload tests ─────────────────────────────────────────────────────

[TestFixture]
public class SkiJumpPayloadTests
{
    private static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    [Test]
    public void SkiJumpPayload_ExposesJumperScoreAndDistance()
    {
        var jumper = C("Kamil");
        var score = new SkiJumpingScore(135.5f, 57.0f, -1.2f, 0.8f);
        var payload = new SkiJumpPayload
        {
            Contestant = jumper,
            Score = score,
            Distance = 135.5f,
        };

        Assert.That(payload.Contestant, Is.SameAs(jumper));
        Assert.That(payload.Score, Is.SameAs(score));
        Assert.That(payload.Distance, Is.EqualTo(135.5f));
    }

    [Test]
    public void SkiJumpPayload_ScoreGetValue_ReturnsPointsSum()
    {
        var payload = new SkiJumpPayload
        {
            Score = new SkiJumpingScore(130f, 57f, -2f, 1f), // 186
        };

        Assert.That(payload.Score!.GetValue(), Is.EqualTo(186).Within(0.01));
    }

    [Test]
    public void Timeline_RecordsMultipleJumps_ReplayGivesCorrectWinner()
    {
        var kamil = C("Kamil");
        var dawid = C("Dawid");
        var stefan = C("Stefan");

        var match = new Match("Qualification", new[] { kamil, dawid, stefan });
        var base_ = new DateTime(2024, 1, 28, 10, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(base_.AddMinutes(0), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(130f, 56f, -1f, 0f), // 185
        }));
        match.Timeline.AddEvent(new InGameEvent(base_.AddMinutes(5), new SkiJumpPayload
        {
            Contestant = dawid,
            Score = new SkiJumpingScore(133f, 57f, 0.5f, 0.3f), // 190.8
        }));
        match.Timeline.AddEvent(new InGameEvent(base_.AddMinutes(10), new SkiJumpPayload
        {
            Contestant = stefan,
            Score = new SkiJumpingScore(128f, 55f, -2f, 0f), // 181
        }));

        var results = new Dictionary<IContestant, double>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is SkiJumpPayload jump && jump.Contestant != null)
                results[jump.Contestant] = jump.Score?.GetValue() ?? 0;
        });

        var winner = results.OrderByDescending(kv => kv.Value).First().Key;
        Assert.That(winner.Name, Is.EqualTo("Dawid"));
    }

    [Test]
    public void Timeline_SecondRoundJump_OverwritesBestScore()
    {
        var kamil = C("Kamil");
        var match = new Match("Final", new[] { kamil });
        var base_ = new DateTime(2024, 1, 28, 10, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(base_.AddMinutes(0), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(120f, 54f, 0f, 0f), // 174
        }));
        match.Timeline.AddEvent(new InGameEvent(base_.AddMinutes(60), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(135f, 57f, 1f, 0.5f), // 193.5
        }));

        var totals = new Dictionary<IContestant, double>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is SkiJumpPayload jump && jump.Contestant != null)
            {
                totals.TryGetValue(jump.Contestant, out var current);
                totals[jump.Contestant] = current + (jump.Score?.GetValue() ?? 0);
            }
        });

        // 174 + 193.5 = 367.5
        Assert.That(totals[kamil], Is.EqualTo(367.5).Within(0.01));
    }
}