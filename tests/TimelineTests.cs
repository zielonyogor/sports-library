using SportsLibrary.FootballClasses;
using SportsLibrary.Model;
using SportsLibrary.SkiJumpingClasses;

namespace tests;

// ─── Core Timeline / IInGameEvent tests ──────────────────────────────────────

[TestFixture]
public class TimelineTests
{
    private static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    private static IScore Pts(float v) => new SkiJumpingScore(v, 0f, 0f, 0f);

    private static MatchSupervisor Referee() =>
        new MatchSupervisor(new Person("Referee", "Smith"));

    // ── add & read ────────────────────────────────────────────────────────────

    [Test]
    public void AddEvent_EventAppearsInEvents()
    {
        var timeline = new Timeline();
        var ev = new InGameEvent(DateTime.Now, new EventPayload());

        timeline.AddEvent(ev);

        Assert.That(timeline.Events, Contains.Item(ev));
    }

    [Test]
    public void AddEvent_MultipleEvents_AllPreserved()
    {
        var timeline = new Timeline();
        var events = Enumerable.Range(0, 5)
            .Select(_ => new InGameEvent(DateTime.Now, new EventPayload()))
            .ToList<IInGameEvent>();

        foreach (var ev in events) timeline.AddEvent(ev);

        Assert.That(timeline.Events.Count, Is.EqualTo(5));
        Assert.That(timeline.Events, Is.EquivalentTo(events));
    }

    [Test]
    public void Events_IsReadOnly_CannotBeModifiedExternally()
    {
        var timeline = new Timeline();
        // IReadOnlyList — no Add/Remove exposed
        Assert.That(timeline.Events, Is.InstanceOf<IReadOnlyList<IInGameEvent>>());
    }

    // ── replay order ──────────────────────────────────────────────────────────

    [Test]
    public void RepeatTimeline_ReplayedInChronologicalOrder()
    {
        var timeline = new Timeline();
        var base_ = new DateTime(2024, 1, 1, 10, 0, 0);

        // Add in reverse order
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(30), new EventPayload()));
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(10), new EventPayload()));
        timeline.AddEvent(new InGameEvent(base_.AddMinutes(50), new EventPayload()));

        var replayed = new List<DateTime>();
        timeline.RepeatTimeline(ev => replayed.Add(ev.Timestamp));

        Assert.That(replayed, Is.Ordered.Ascending);
    }

    [Test]
    public void RepeatTimeline_EmptyTimeline_CallbackNeverInvoked()
    {
        var timeline = new Timeline();
        int calls = 0;

        timeline.RepeatTimeline(_ => calls++);

        Assert.That(calls, Is.EqualTo(0));
    }

    [Test]
    public void RepeatTimeline_PayloadsAccessibleDuringReplay()
    {
        var timeline = new Timeline();
        var contestant = C("Kamil");
        var payload = new EventPayload { Contestant = contestant, Score = Pts(200f) };
        timeline.AddEvent(new InGameEvent(DateTime.Now, payload));

        IEventPayload? captured = null;
        timeline.RepeatTimeline(ev => captured = ev.GetEvent());

        Assert.That(captured, Is.SameAs(payload));
        var ep = (EventPayload)captured!;
        Assert.That(ep.Contestant, Is.SameAs(contestant));
        Assert.That(ep.Score!.GetValue(), Is.EqualTo(200).Within(0.01));
    }

    // ── EventPayload properties ───────────────────────────────────────────────

    [Test]
    public void EventPayload_AllPropertiesNullByDefault()
    {
        var payload = new EventPayload();

        Assert.That(payload.Score, Is.Null);
        Assert.That(payload.Contestant, Is.Null);
        Assert.That(payload.Referee, Is.Null);
    }

    [Test]
    public void EventPayload_PropertiesSetViaInitializer()
    {
        var contestant = C("Stefan");
        var referee = Referee();
        var score = Pts(150f);

        var payload = new EventPayload
        {
            Contestant = contestant,
            Referee = referee,
            Score = score,
        };

        Assert.That(payload.Contestant, Is.SameAs(contestant));
        Assert.That(payload.Referee, Is.SameAs(referee));
        Assert.That(payload.Score, Is.SameAs(score));
    }
}

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

// ─── Football timeline tests ──────────────────────────────────────────────────

[TestFixture]
public class FootballTimelineTests
{
    private static IContestant C(string name) =>
        new TeamContestant(name);

    private static MatchSupervisor Ref() =>
        new MatchSupervisor(new Person("Referee", "Smith"));

    [Test]
    public void FootballGoalPayload_ExposesScorer()
    {
        var red = C("Red");
        var payload = new FootballGoalPayload
        {
            Contestant = red,
            Score = new FootballMatchScore { GoalsScored = 1, Result = MatchOutcome.Win },
            Minute = 35,
        };

        Assert.That(payload.Contestant, Is.SameAs(red));
        Assert.That(payload.Minute, Is.EqualTo(35));
        Assert.That(payload.Score!.GetValue(), Is.EqualTo(1));
    }

    [Test]
    public void FootballGoalPayload_ExposesAssistProvider()
    {
        var scorer = C("Red");
        var assister = C("Blue");
        var payload = new FootballGoalPayload
        {
            Contestant = scorer,
            AssistProvider = assister,
            Minute = 72,
        };

        Assert.That(payload.AssistProvider, Is.SameAs(assister));
    }

    [Test]
    public void FootballCardPayload_CardTypeAndMinuteExposed()
    {
        var payload = new FootballCardPayload
        {
            Contestant = C("Blue"),
            CardType = CardType.Yellow,
            Minute = 55,
        };

        Assert.That(payload.CardType, Is.EqualTo(CardType.Yellow));
        Assert.That(payload.Minute, Is.EqualTo(55));
    }

    [Test]
    public void FootballCardPayload_RedCard_IdentifiedCorrectly()
    {
        var payload = new FootballCardPayload
        {
            Contestant = C("Red"),
            CardType = CardType.Red,
            Minute = 88,
        };

        Assert.That(payload.CardType, Is.EqualTo(CardType.Red));
    }

    [Test]
    public void Timeline_RecordsFullMatchHistory_GoalsAndCardsInOrder()
    {
        var red = C("Red");
        var blue = C("Blue");
        var referee = Ref();
        var match = new Match("Group A", new[] { red, blue });
        var kickOff = new DateTime(2024, 7, 15, 15, 0, 0);

        // Build timeline: goal at 22', yellow card at 45', goal at 67', red card at 88'
        match.Timeline.AddEvent(new InGameEvent(kickOff.AddMinutes(22), new FootballGoalPayload
        {
            Contestant = red, Minute = 22, Referee = referee,
            Score = new FootballMatchScore { GoalsScored = 1, Result = MatchOutcome.Win },
        }));
        match.Timeline.AddEvent(new InGameEvent(kickOff.AddMinutes(88), new FootballCardPayload
        {
            Contestant = red, CardType = CardType.Red, Minute = 88, Referee = referee,
        }));
        match.Timeline.AddEvent(new InGameEvent(kickOff.AddMinutes(45), new FootballCardPayload
        {
            Contestant = blue, CardType = CardType.Yellow, Minute = 45, Referee = referee,
        }));
        match.Timeline.AddEvent(new InGameEvent(kickOff.AddMinutes(67), new FootballGoalPayload
        {
            Contestant = blue, Minute = 67, Referee = referee,
            Score = new FootballMatchScore { GoalsScored = 1, Result = MatchOutcome.Draw },
        }));

        var log = new List<(int minute, string type, string team)>();
        match.Timeline.RepeatTimeline(ev =>
        {
            switch (ev.GetEvent())
            {
                case FootballGoalPayload g:
                    log.Add((g.Minute, "goal", g.Contestant!.Name));
                    break;
                case FootballCardPayload c:
                    log.Add((c.Minute, c.CardType == CardType.Yellow ? "yellow" : "red", c.Contestant!.Name));
                    break;
            }
        });

        // Replay is in chronological order regardless of insertion order
        Assert.That(log.Select(e => e.minute), Is.Ordered.Ascending);
        Assert.That(log, Has.Count.EqualTo(4));
        Assert.That(log[0], Is.EqualTo((22, "goal", "Red")));
        Assert.That(log[1], Is.EqualTo((45, "yellow", "Blue")));
        Assert.That(log[2], Is.EqualTo((67, "goal", "Blue")));
        Assert.That(log[3], Is.EqualTo((88, "red", "Red")));
    }

    [Test]
    public void Timeline_GoalCount_CorrectlyDerivedFromReplay()
    {
        var red = C("Red");
        var blue = C("Blue");
        var match = new Match("Final", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        // Red scores twice; Blue scores once
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(10), new FootballGoalPayload { Contestant = red }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(55), new FootballGoalPayload { Contestant = blue }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(80), new FootballGoalPayload { Contestant = red }));

        var goals = new Dictionary<IContestant, int>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is FootballGoalPayload g && g.Contestant != null)
            {
                goals.TryGetValue(g.Contestant, out var n);
                goals[g.Contestant] = n + 1;
            }
        });

        Assert.That(goals[red], Is.EqualTo(2));
        Assert.That(goals[blue], Is.EqualTo(1));
    }

    [Test]
    public void Timeline_CardAccumulation_TwoYellowsMakeRed()
    {
        var player = C("Blue");
        var match = new Match("Semifinal", new[] { player });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(30), new FootballCardPayload
        {
            Contestant = player, CardType = CardType.Yellow, Minute = 30,
        }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballCardPayload
        {
            Contestant = player, CardType = CardType.Yellow, Minute = 60,
        }));

        var yellowCount = new Dictionary<IContestant, int>();
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is FootballCardPayload card
                && card.CardType == CardType.Yellow
                && card.Contestant != null)
            {
                yellowCount.TryGetValue(card.Contestant, out var n);
                yellowCount[card.Contestant] = n + 1;
            }
        });

        Assert.That(yellowCount[player], Is.EqualTo(2),
            "Two yellow cards in the timeline — player should be considered sent off");
    }

    // ── FootballerInjuredPayload ───────────────────────────────────────────────

    [Test]
    public void FootballerInjuredPayload_ExposesMinuteAndDescription()
    {
        var payload = new FootballerInjuredPayload
        {
            Contestant = C("Red"),
            Minute = 34,
            Description = "Hamstring strain",
        };

        Assert.That(payload.Minute, Is.EqualTo(34));
        Assert.That(payload.Description, Is.EqualTo("Hamstring strain"));
    }

    [Test]
    public void FootballerInjuredPayload_ExposesInjuredPlayerAndMinute()
    {
        var player = C("Blue");
        var payload = new FootballerInjuredPayload
        {
            Contestant = player,
            Minute = 67,
            Description = "Knee collision",
        };

        Assert.That(payload.Contestant, Is.SameAs(player));
        Assert.That(payload.Minute, Is.EqualTo(67));
        Assert.That(payload.Description, Is.EqualTo("Knee collision"));
    }

    [Test]
    public void Timeline_InjuryEvent_CountedSeparatelyFromGoalsAndCards()
    {
        var red = C("Red");
        var blue = C("Blue");
        var match = new Match("Group B", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(22),
            new FootballGoalPayload { Contestant = red, Minute = 22 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(35),
            new FootballerInjuredPayload { Contestant = blue, Minute = 35, Description = "Ankle" }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45),
            new FootballCardPayload { Contestant = red, CardType = CardType.Yellow, Minute = 45 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(78),
            new FootballerInjuredPayload { Contestant = red, Minute = 78 }));

        int goals = 0, injuries = 0, cards = 0;
        match.Timeline.RepeatTimeline(ev =>
        {
            switch (ev.GetEvent())
            {
                case FootballGoalPayload:       goals++;    break;
                case FootballerInjuredPayload:  injuries++; break;
                case FootballCardPayload:       cards++;    break;
            }
        });

        Assert.That(goals,    Is.EqualTo(1));
        Assert.That(injuries, Is.EqualTo(2));
        Assert.That(cards,    Is.EqualTo(1));
    }
}

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

        match.Timeline.AddEvent(new InGameEvent(t,               new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
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
        var red  = C("Red");
        var blue = C("Blue");
        var match = new Match("Group A", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        // FirstHalf: Red scores at 22'
        match.Timeline.AddEvent(new InGameEvent(t,               new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(22), new FootballGoalPayload  { Contestant = red,  Minute = 22 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45), new FootballPeriodPayload { Period = MatchPeriod.HalfTime }));
        // SecondHalf: Blue scores at 67', Red scores at 88'
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballPeriodPayload { Period = MatchPeriod.SecondHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(67), new FootballGoalPayload  { Contestant = blue, Minute = 67 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(88), new FootballGoalPayload  { Contestant = red,  Minute = 88 }));
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

        Assert.That(goalsByPeriod[MatchPeriod.FirstHalf],  Is.EqualTo(1));
        Assert.That(goalsByPeriod[MatchPeriod.SecondHalf], Is.EqualTo(2));
    }

    [Test]
    public void Timeline_ExtraTimePeriods_TrackGoalInExtraTime()
    {
        var red  = C("Red");
        var blue = C("Blue");
        var match = new Match("Semifinal", new[] { red, blue });
        var t = new DateTime(2024, 7, 15, 15, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t,                new FootballPeriodPayload { Period = MatchPeriod.FirstHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(45), new FootballPeriodPayload { Period = MatchPeriod.HalfTime }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(60), new FootballPeriodPayload { Period = MatchPeriod.SecondHalf }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(105), new FootballPeriodPayload { Period = MatchPeriod.FullTime }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(120), new FootballPeriodPayload { Period = MatchPeriod.ExtraTimeFirst }));
        // Golden goal in extra time at 95'
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(125), new FootballGoalPayload  { Contestant = red, Minute = 95 }));

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
        var kamil  = C("Kamil");
        var dawid  = C("Dawid");
        var stefan = C("Stefan");
        var match  = new Match("Qualification", new[] { kamil, dawid, stefan });
        var judge  = GateJudge();
        var t      = new DateTime(2024, 1, 28, 10, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(0), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(130f, 56f, -1f, 0f),   // 185 pts, gate 14
            Distance = 130f,
        }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(5), new GateLoweredPayload
        {
            NewGate = 12, GatesLowered = 2, CompensationPerJump = 7.2f, Referee = judge,
        }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(10), new SkiJumpPayload
        {
            Contestant = dawid,
            Score = new SkiJumpingScore(127f, 55.5f, 0.5f, 7.2f),  // base + compensation
            Distance = 127f,
        }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(15), new SkiJumpPayload
        {
            Contestant = stefan,
            Score = new SkiJumpingScore(128f, 55f, -2f, 7.2f),
            Distance = 128f,
        }));

        // Replay: track current gate compensation and accumulate each jumper's final score
        float activeCompensation = 0f;
        var finalScores = new Dictionary<IContestant, double>();
        match.Timeline.RepeatTimeline(ev =>
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
        });

        // Kamil jumped before the gate change → no gate compensation baked in
        Assert.That(finalScores[kamil],  Is.EqualTo(185).Within(0.01));
        // Dawid and Stefan jumped after → their scores include +7.2 compensation
        Assert.That(finalScores[dawid],  Is.EqualTo(190.2).Within(0.01));
        Assert.That(finalScores[stefan], Is.EqualTo(188.2).Within(0.01));
        // Compensation was captured during replay
        Assert.That(activeCompensation,  Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void Timeline_MultipleGateChanges_LastChangeWins()
    {
        var kamil = C("Kamil");
        var match = new Match("Final", new[] { kamil });
        var t = new DateTime(2024, 1, 28, 10, 0, 0);

        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(5),
            new GateLoweredPayload { NewGate = 13, GatesLowered = 1, CompensationPerJump = 3.6f }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(10),
            new GateLoweredPayload { NewGate = 11, GatesLowered = 2, CompensationPerJump = 7.2f }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(15), new SkiJumpPayload
        {
            Contestant = kamil,
            Score = new SkiJumpingScore(125f, 55f, 0f, 7.2f),  // second change applies
            Distance = 125f,
        }));

        float compensation = 0f;
        match.Timeline.RepeatTimeline(ev =>
        {
            if (ev.GetEvent() is GateLoweredPayload gate)
                compensation = gate.CompensationPerJump;
        });

        Assert.That(compensation, Is.EqualTo(7.2f).Within(0.001f));
    }

    [Test]
    public void Timeline_EventTypeDistribution_CorrectCounts()
    {
        var match = new Match("Competition", new[] { C("A"), C("B") });
        var t = new DateTime(2024, 1, 28, 10, 0, 0);

        // 3 jumps, 2 gate changes
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(0),  new SkiJumpPayload { Contestant = C("A") }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(3),  new GateLoweredPayload { NewGate = 13, GatesLowered = 1 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(6),  new SkiJumpPayload { Contestant = C("B") }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(9),  new GateLoweredPayload { NewGate = 11, GatesLowered = 2 }));
        match.Timeline.AddEvent(new InGameEvent(t.AddMinutes(12), new SkiJumpPayload { Contestant = C("A") }));

        int jumps = 0, gateChanges = 0;
        match.Timeline.RepeatTimeline(ev =>
        {
            switch (ev.GetEvent())
            {
                case SkiJumpPayload:    jumps++;       break;
                case GateLoweredPayload: gateChanges++; break;
            }
        });

        Assert.That(jumps,       Is.EqualTo(3));
        Assert.That(gateChanges, Is.EqualTo(2));
    }
}
