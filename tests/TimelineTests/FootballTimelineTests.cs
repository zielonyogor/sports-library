using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

// ─── Football timeline tests ──────────────────────────────────────────────────

[TestFixture]
public class FootballTimelineTests
{
    private static IContestant C(string name) =>
        new TeamContestant(name);

    private static MatchSupervisor R(string name) =>
        new MatchSupervisor(new Person(name, ""));

    [Test]
    public void FootballGoalPayload_ExposesScorer()
    {
        var red = C("Red");
        var payload = new FootballGoalPayload
        {
            Contestant = red,
            Score = new FootballMatchScore(1),
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
            Referee = R("Referee"),
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
            Referee = R("Referee")
        };

        Assert.That(payload.CardType, Is.EqualTo(CardType.Red));
    }

    [Test]
    public void Timeline_RecordsFullMatchHistory_GoalsAndCardsInOrder()
    {
        var red = C("Red");
        var blue = C("Blue");
        var referee = R("Referee");
        var kickOff = new DateTime(2024, 7, 15, 15, 0, 0);
        var match = new Match("Group A", new[] { red, blue }, kickOff.AddMinutes(-1));
        match.Start(kickOff);

        // Build timeline: goal at 22', yellow card at 45', goal at 67', red card at 88'
        match.RecordEvent(new InGameEvent(kickOff.AddMinutes(22), new FootballGoalPayload
        {
            Contestant = red,
            Minute = 22,
            Referee = referee,
            Score = new FootballMatchScore(1),
        }));
        match.RecordEvent(new InGameEvent(kickOff.AddMinutes(45), new FootballCardPayload
        {
            Contestant = blue,
            CardType = CardType.Yellow,
            Minute = 45,
            Referee = referee,
        }));
        match.RecordEvent(new InGameEvent(kickOff.AddMinutes(67), new FootballGoalPayload
        {
            Contestant = blue,
            Minute = 67,
            Referee = referee,
            Score = new FootballMatchScore(1),
        }));
        match.RecordEvent(new InGameEvent(kickOff.AddMinutes(88), new FootballCardPayload
        {
            Contestant = red,
            CardType = CardType.Red,
            Minute = 88,
            Referee = referee,
        }));

        var log = new List<(int minute, string type, string team)>();
        foreach (var ev in match.Timeline.Events)
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
                }

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
        var t = new DateTime(2024, 7, 15, 15, 0, 0);
        var match = new Match("Final", new[] { red, blue }, t.AddMinutes(-1));
        match.Start(t);

        // Red scores twice; Blue scores once
        match.RecordEvent(new InGameEvent(t.AddMinutes(10), new FootballGoalPayload { Contestant = red }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(55), new FootballGoalPayload { Contestant = blue }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(80), new FootballGoalPayload { Contestant = red }));

        var goals = new Dictionary<IContestant, int>();
        foreach (var ev in match.Timeline.Events)
        {
            if (ev.GetEvent() is FootballGoalPayload g && g.Contestant != null)
            {
                goals.TryGetValue(g.Contestant, out var n);
                goals[g.Contestant] = n + 1;
            }
        }

        Assert.That(goals[red], Is.EqualTo(2));
        Assert.That(goals[blue], Is.EqualTo(1));
    }

    [Test]
    public void Timeline_CardAccumulation_TwoYellowsMakeRed()
    {
        var player = C("Blue");
        var t = new DateTime(2024, 7, 15, 15, 0, 0);
        var match = new Match("Semifinal", new[] { player }, t.AddMinutes(-1));
        match.Start(t);

        match.RecordEvent(new InGameEvent(t.AddMinutes(30), new FootballCardPayload
        {
            Contestant = player,
            CardType = CardType.Yellow,
            Minute = 30,
            Referee = R("Referee"),
        }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(60), new FootballCardPayload
        {
            Contestant = player,
            CardType = CardType.Yellow,
            Minute = 60,
            Referee = R("Referee"),
        }));

        var yellowCount = new Dictionary<IContestant, int>();
        foreach (var ev in match.Timeline.Events)
        {
            if (ev.GetEvent() is FootballCardPayload card
                && card.CardType == CardType.Yellow
                && card.Contestant != null)
            {
                yellowCount.TryGetValue(card.Contestant, out var n);
                yellowCount[card.Contestant] = n + 1;
            }
        }

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
        var t = new DateTime(2024, 7, 15, 15, 0, 0);
        var match = new Match("Group B", new[] { red, blue }, t.AddMinutes(-1));
        match.Start(t);

        match.RecordEvent(new InGameEvent(t.AddMinutes(22),
            new FootballGoalPayload { Contestant = red, Minute = 22 }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(35),
            new FootballerInjuredPayload { Contestant = blue, Minute = 35, Description = "Ankle" }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(45),
            new FootballCardPayload { Contestant = red, CardType = CardType.Yellow, Minute = 45, Referee = R("Referee") }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(78),
            new FootballerInjuredPayload { Contestant = red, Minute = 78 }));

        int goals = 0, injuries = 0, cards = 0;
        foreach (var ev in match.Timeline.Events)
        {
            switch (ev.GetEvent())
            {
                case FootballGoalPayload: goals++; break;
                case FootballerInjuredPayload: injuries++; break;
                case FootballCardPayload: cards++; break;
            }
        }

        Assert.That(goals, Is.EqualTo(1));
        Assert.That(injuries, Is.EqualTo(2));
        Assert.That(cards, Is.EqualTo(1));
    }
}