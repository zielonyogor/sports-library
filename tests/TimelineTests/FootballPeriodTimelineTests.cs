using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── Football period timeline tests ──────────────────────────────────────────

[TestFixture]
public class FootballPeriodTimelineTests
{
    private static IContestant C(string name) => new TeamContestant(name);

    [Test]
    public void Timeline_PeriodSequence_ReplayedInOrder()
    {
        var match = new Match("Final", new[] { C("Red"), C("Blue") });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t, new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45), new FootballPeriodPayload { Period = MatchPeriod.HalfTime }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballPeriodPayload { Period = MatchPeriod.SecondHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(105), new FootballPeriodPayload { Period = MatchPeriod.FullTime }));

        var periods = new List<MatchPeriod>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is FootballPeriodPayload p)
                periods.Add(p.Period);
        });

        Assert.That(periods, Is.EqualTo(new[]
        {
            MatchPeriod.FirstHalf, MatchPeriod.HalfTime,
            MatchPeriod.SecondHalf, MatchPeriod.FullTime,
        }));
    }

    [Test]
    public void Timeline_GoalsAttributedToCorrectPeriod()
    {
        var red = C("Red");
        var blue = C("Blue");
        var match = new Match("Group A", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        // FirstHalf: Red scores at 22'
        match.Timeline.AddEvent(new InGameEvent(t, new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(22), new FootballGoalPayload { Contestant = red, Minute = 22 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45), new FootballPeriodPayload { Period = MatchPeriod.HalfTime }));
        // SecondHalf: Blue scores at 67', Red scores at 88'
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballPeriodPayload { Period = MatchPeriod.SecondHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(67), new FootballGoalPayload { Contestant = blue, Minute = 67 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(88), new FootballGoalPayload { Contestant = red, Minute = 88 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(105), new FootballPeriodPayload { Period = MatchPeriod.FullTime }));

        var currentPeriod = MatchPeriod.FirstHalf;
        var goalsByPeriod = new Dictionary<MatchPeriod, int>();
        match.Timeline.RepeatTimeline(ev =>
        {
            switch (ev.GetEvent())
            {
                case FootballPeriodPayload p:
                    currentPeriod = p.Period;
                    break;
                case FootballGoalPayload:
                    goalsByPeriod.TryGetValue(currentPeriod, out var n);
                    goalsByPeriod[currentPeriod] = n + 1;
                    break;
            }
        });

        Assert.That(goalsByPeriod[MatchPeriod.FirstHalf], Is.EqualTo(1));
        Assert.That(goalsByPeriod[MatchPeriod.SecondHalf], Is.EqualTo(2));
    }

    [Test]
    public void Timeline_ExtraTimePeriods_TrackGoalInExtraTime()
    {
        var red = C("Red");
        var blue = C("Blue");
        var match = new Match("Semifinal", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t, new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45), new FootballPeriodPayload { Period = MatchPeriod.HalfTime }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballPeriodPayload { Period = MatchPeriod.SecondHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(105), new FootballPeriodPayload { Period = MatchPeriod.FullTime }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(120), new FootballPeriodPayload { Period = MatchPeriod.ExtraTimeFirst }));
        // Golden goal in extra time at 95'
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(125), new FootballGoalPayload { Contestant = red, Minute = 95 }));

        var currentPeriod = MatchPeriod.FirstHalf;
        IContestant? extraTimeScorer = null;
        match.Timeline.RepeatTimeline(ev =>
        {
            switch (ev.GetEvent())
            {
                case FootballPeriodPayload p:
                    currentPeriod = p.Period;
                    break;
                case FootballGoalPayload g when currentPeriod is MatchPeriod.ExtraTimeFirst or MatchPeriod.ExtraTimeSecond:
                    extraTimeScorer = g.Contestant;
                    break;
            }
        });

        Assert.That(extraTimeScorer, Is.SameAs(red));
    }
}