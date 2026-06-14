using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

// ─── New payloads ─────────────────────────────────────────────────────────────

[TestFixture]
public class NewPayloadTests
{
    private static IContestant T(string name) => new TeamContestant(name);
    private static IContestant C(string name) => new SingleContestant(name, new Person(name, ""));

    [Test]
    public void FootballSubstitutionPayload_ExposesPlayersAndMinute()
    {
        var playerIn = T("Sub");
        var playerOff = T("Starter");
        var payload = new FootballSubstitutionPayload
        {
            PlayerIn = playerIn,
            PlayerOff = playerOff,
            Minute = 65,
        };

        Assert.That(payload.PlayerIn, Is.SameAs(playerIn));
        Assert.That(payload.PlayerOff, Is.SameAs(playerOff));
        Assert.That(payload.Minute, Is.EqualTo(65));
    }

    [Test]
    public void FootballPeriodPayload_ExposesPeriod()
    {
        var payload = new FootballPeriodPayload { Period = MatchPeriod.HalfTime };

        Assert.That(payload.Period, Is.EqualTo(MatchPeriod.HalfTime));
    }

    [Test]
    public void FootballPeriodPayload_AllPeriodValuesAreValid()
    {
        foreach (var period in Enum.GetValues<MatchPeriod>())
        {
            var payload = new FootballPeriodPayload { Period = period };
            Assert.That(payload.Period, Is.EqualTo(period));
        }
    }

    [Test]
    public void FootballPenaltyPayload_InMatchSpotKick_HasMinute()
    {
        var player = T("Player");
        var payload = new FootballPenaltyPayload { Contestant = player, Scored = true, Minute = 88 };

        Assert.That(payload.Contestant, Is.SameAs(player));
        Assert.That(payload.Scored, Is.True);
        Assert.That(payload.Minute, Is.EqualTo(88));
    }

    [Test]
    public void FootballPenaltyPayload_ShootoutAttempt_MinuteIsNull()
    {
        var payload = new FootballPenaltyPayload { Contestant = T("Player"), Scored = false };

        Assert.That(payload.Scored, Is.False);
        Assert.That(payload.Minute, Is.Null);
    }

    [Test]
    public void Timeline_PenaltyShootout_ScoreAndMissSequenceTracked()
    {
        var red = T("Red");
        var blue = T("Blue");
        var t = new DateTime(2024, 7, 15, 17, 0, 0);
        var match = new Match("Final", new[] { red, blue }, t.AddMinutes(-1));
        match.Start(t);

        match.RecordEvent(new InGameEvent(t.AddSeconds(1), new FootballPeriodPayload { Period = MatchPeriod.PenaltyShootout }));
        // Red scores, Blue scores, Red scores, Blue misses, Red scores
        match.RecordEvent(new InGameEvent(t.AddSeconds(30), new FootballPenaltyPayload { Contestant = red, Scored = true }));
        match.RecordEvent(new InGameEvent(t.AddSeconds(60), new FootballPenaltyPayload { Contestant = blue, Scored = true }));
        match.RecordEvent(new InGameEvent(t.AddSeconds(90), new FootballPenaltyPayload { Contestant = red, Scored = true }));
        match.RecordEvent(new InGameEvent(t.AddSeconds(120), new FootballPenaltyPayload { Contestant = blue, Scored = false }));
        match.RecordEvent(new InGameEvent(t.AddSeconds(150), new FootballPenaltyPayload { Contestant = red, Scored = true }));

        var scored = new Dictionary<IContestant, int>();
        var missed = new Dictionary<IContestant, int>();
        foreach (var ev in match.Timeline.Events)
        {
            if (ev.GetEvent() is FootballPenaltyPayload p && p.Contestant != null)
            {
                if (p.Scored) { scored.TryGetValue(p.Contestant, out var s); scored[p.Contestant] = s + 1; }
                else { missed.TryGetValue(p.Contestant, out var m); missed[p.Contestant] = m + 1; }
            }
        }

        Assert.That(scored[red], Is.EqualTo(3));
        Assert.That(scored[blue], Is.EqualTo(1));
        Assert.That(missed[blue], Is.EqualTo(1));
        Assert.That(missed.ContainsKey(red), Is.False);
    }

    [Test]
    public void SkiJumpingDisqualificationPayload_ExposesContestantAndReason()
    {
        var kamil = C("Kamil");
        var payload = new SkiJumpingDisqualificationPayload
        {
            Contestant = kamil,
            Reason = "Suit violation",
        };

        Assert.That(payload.Contestant, Is.SameAs(kamil));
        Assert.That(payload.Reason, Is.EqualTo("Suit violation"));
    }

    [Test]
    public void SkiJumpingDisqualificationPayload_NullableReason_DefaultsToNull()
    {
        var payload = new SkiJumpingDisqualificationPayload { Contestant = C("Kamil") };
        Assert.That(payload.Reason, Is.Null);
    }
}