using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace tests;

// ─── Timeline: GetEventsByPayloadType + RemoveLastEvent + null validation ──────

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
        timeline.AddEvent(new InGameEvent(t,               new FootballGoalPayload { Contestant = red, Minute = 10 }));
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
    public void RemoveLastEvent_RemovesTheLastAddedEvent()
    {
        var timeline = new Timeline();
        var ev1 = new InGameEvent(DateTime.Now,               new EventPayload());
        var ev2 = new InGameEvent(DateTime.Now.AddMinutes(1), new EventPayload());
        timeline.AddEvent(ev1);
        timeline.AddEvent(ev2);

        var removed = timeline.RemoveLastEvent();

        Assert.That(removed, Is.True);
        Assert.That(timeline.Events.Count, Is.EqualTo(1));
        Assert.That(timeline.Events[0], Is.SameAs(ev1));
    }

    [Test]
    public void RemoveLastEvent_EmptyTimeline_ReturnsFalse()
    {
        var timeline = new Timeline();
        Assert.That(timeline.RemoveLastEvent(), Is.False);
        Assert.That(timeline.Events, Is.Empty);
    }

    [Test]
    public void RemoveLastEvent_AllEventsRemoved_TimelineIsEmpty()
    {
        var timeline = new Timeline();
        timeline.AddEvent(new InGameEvent(DateTime.Now, new EventPayload()));
        timeline.RemoveLastEvent();
        Assert.That(timeline.Events, Is.Empty);
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

// ─── Match: GetWinner + GetCurrentScore ───────────────────────────────────────

[TestFixture]
public class MatchWinnerTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    [Test]
    public void GetWinner_ReturnsHighestScoringContestant()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        match.Statistics[red]  = new FootballMatchScore { GoalsScored = 3 };
        match.Statistics[blue] = new FootballMatchScore { GoalsScored = 1 };

        Assert.That(match.GetWinner(), Is.SameAs(red));
    }

    [Test]
    public void GetWinner_TiedScore_ReturnsPenaltyWinner()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        match.Statistics[red]  = new FootballMatchScore { GoalsScored = 2 };
        match.Statistics[blue] = new FootballMatchScore { GoalsScored = 2 };
        match.PenaltyWinner = blue;

        Assert.That(match.GetWinner(), Is.SameAs(blue));
    }

    [Test]
    public void GetWinner_TiedWithNoPenaltyWinner_ReturnsNull()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        match.Statistics[red]  = new FootballMatchScore { GoalsScored = 1 };
        match.Statistics[blue] = new FootballMatchScore { GoalsScored = 1 };

        Assert.That(match.GetWinner(), Is.Null);
    }

    [Test]
    public void GetWinner_NoStatistics_ReturnsNull()
    {
        var match = new Match("Test", new[] { T("A") });
        Assert.That(match.GetWinner(), Is.Null);
    }

    [Test]
    public void GetCurrentScore_ReturnsScoreForKnownContestant()
    {
        var team = T("Red");
        var match = new Match("Test", new[] { team });
        var score = new FootballMatchScore { GoalsScored = 2 };
        match.Statistics[team] = score;

        Assert.That(match.GetCurrentScore(team), Is.SameAs(score));
    }

    [Test]
    public void GetCurrentScore_UnknownContestant_ReturnsNull()
    {
        var match = new Match("Test", new[] { T("A") });
        Assert.That(match.GetCurrentScore(T("Unknown")), Is.Null);
    }

    [Test]
    public void GetCurrentScore_NoStatistics_ReturnsNull()
    {
        var team = T("Red");
        var match = new Match("Test", new[] { team });
        Assert.That(match.GetCurrentScore(team), Is.Null);
    }
}

// ─── FootballMatchController ──────────────────────────────────────────────────

[TestFixture]
public class FootballMatchControllerTests
{
    private static IContestant T(string name) => new TeamContestant(name);
    private static MatchSupervisor R(string name) => new MatchSupervisor(new Person(name, ""));

    [Test]
    public void GetGoalCount_CountsGoalsPerTeam()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        var t = DateTime.Now;
        match.Timeline.AddEvent(new InGameEvent(t,               new FootballGoalPayload { Contestant = red,  Minute = 10 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(20), new FootballGoalPayload { Contestant = blue, Minute = 30 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(40), new FootballGoalPayload { Contestant = red,  Minute = 50 }));

        var ctrl = new FootballMatchController(match);

        Assert.That(ctrl.GetGoalCount(red),  Is.EqualTo(2));
        Assert.That(ctrl.GetGoalCount(blue), Is.EqualTo(1));
    }

    [Test]
    public void GetGoalCount_NoGoals_ReturnsZero()
    {
        var red = T("Red");
        var match = new Match("Test", new[] { red });
        Assert.That(new FootballMatchController(match).GetGoalCount(red), Is.EqualTo(0));
    }

    [Test]
    public void IsPlayerSentOff_TwoYellowCards_ReturnsTrue()
    {
        var player = T("Blue");
        var match = new Match("Test", new[] { player });
        var t = DateTime.Now;
        var referee = R("Referee");
        match.Timeline.AddEvent(new InGameEvent(t,               new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 30, Referee = referee }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(30), new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 60, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.True);
    }

    [Test]
    public void IsPlayerSentOff_DirectRedCard_ReturnsTrue()
    {
        var player = T("Blue");
        var match = new Match("Test", new[] { player });
        var referee = R("Referee");
        match.Timeline.AddEvent(new InGameEvent(DateTime.Now,
            new FootballCardPayload { Contestant = player, CardType = CardType.Red, Minute = 50, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.True);
    }

    [Test]
    public void IsPlayerSentOff_OneYellowCard_ReturnsFalse()
    {
        var player = T("Blue");
        var match = new Match("Test", new[] { player });
        var referee = R("Referee");
        match.Timeline.AddEvent(new InGameEvent(DateTime.Now,
            new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 30, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.False);
    }

    [Test]
    public void GetSubstitutions_CountsCorrectly()
    {
        var red = T("Red");
        var match = new Match("Test", new[] { red });
        var playerOn  = T("Sub");
        var playerOff = T("Starter");
        match.Timeline.AddEvent(new InGameEvent(DateTime.Now,
            new FootballSubstitutionPayload { Contestant = playerOn, PlayerOff = playerOff, Minute = 65 }));

        var subs = new FootballMatchController(match).GetSubstitutions();

        Assert.That(subs.Count, Is.EqualTo(1));
        Assert.That(subs[0].Minute, Is.EqualTo(65));
        Assert.That(subs[0].PlayerOff, Is.SameAs(playerOff));
    }
}

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
        match.Timeline.AddEvent(new InGameEvent(t,               new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) }));  // 187
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(135f, 58f, 1f, 0f) })); // 194

        Assert.That(new SkiJumpMatchController(match).GetTotalScore(kamil), Is.EqualTo(381).Within(0.01));
    }

    [Test]
    public void GetBestJump_ReturnsMaximumJumpScore()
    {
        var kamil = C("Kamil");
        var match = new Match("Finals", new[] { kamil });
        var t = DateTime.Now;
        match.Timeline.AddEvent(new InGameEvent(t,               new SkiJumpPayload { Contestant = kamil, Score = new SkiJumpingScore(130f, 57f, 0f, 0f) })); // 187
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

// ─── New payloads ─────────────────────────────────────────────────────────────

[TestFixture]
public class NewPayloadTests
{
    private static IContestant T(string name) => new TeamContestant(name);
    private static IContestant C(string name) => new SingleContestant(name, new Person(name, ""));

    [Test]
    public void FootballSubstitutionPayload_ExposesPlayersAndMinute()
    {
        var playerOn  = T("Sub");
        var playerOff = T("Starter");
        var payload = new FootballSubstitutionPayload
        {
            Contestant = playerOn,
            PlayerOff  = playerOff,
            Minute     = 65,
        };

        Assert.That(payload.Contestant, Is.SameAs(playerOn));
        Assert.That(payload.PlayerOff,  Is.SameAs(playerOff));
        Assert.That(payload.Minute,     Is.EqualTo(65));
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
        Assert.That(payload.Scored,     Is.True);
        Assert.That(payload.Minute,     Is.EqualTo(88));
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
        var red  = T("Red");
        var blue = T("Blue");
        var match = new Match("Final", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 17, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t, new FootballPeriodPayload { Period = MatchPeriod.PenaltyShootout }));
        // Red scores, Blue scores, Red scores, Blue misses, Red scores
        match.Timeline.AddEvent(new InGameEvent(t.AddSeconds(30),  new FootballPenaltyPayload { Contestant = red,  Scored = true  }));
        match.Timeline.AddEvent(new InGameEvent(t.AddSeconds(60),  new FootballPenaltyPayload { Contestant = blue, Scored = true  }));
        match.Timeline.AddEvent(new InGameEvent(t.AddSeconds(90),  new FootballPenaltyPayload { Contestant = red,  Scored = true  }));
        match.Timeline.AddEvent(new InGameEvent(t.AddSeconds(120), new FootballPenaltyPayload { Contestant = blue, Scored = false }));
        match.Timeline.AddEvent(new InGameEvent(t.AddSeconds(150), new FootballPenaltyPayload { Contestant = red,  Scored = true  }));

        var scored = new Dictionary<IContestant, int>();
        var missed = new Dictionary<IContestant, int>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is FootballPenaltyPayload p && p.Contestant != null)
            {
                if (p.Scored) { scored.TryGetValue(p.Contestant, out var s); scored[p.Contestant] = s + 1; }
                else          { missed.TryGetValue(p.Contestant, out var m); missed[p.Contestant] = m + 1; }
            }
        });

        Assert.That(scored[red],  Is.EqualTo(3));
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
            Reason     = "Suit violation",
        };

        Assert.That(payload.Contestant, Is.SameAs(kamil));
        Assert.That(payload.Reason,     Is.EqualTo("Suit violation"));
    }

    [Test]
    public void SkiJumpingDisqualificationPayload_NullableReason_DefaultsToNull()
    {
        var payload = new SkiJumpingDisqualificationPayload { Contestant = C("Kamil") };
        Assert.That(payload.Reason, Is.Null);
    }
}
