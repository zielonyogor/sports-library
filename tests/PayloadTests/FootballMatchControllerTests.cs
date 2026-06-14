using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

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
        match.RecordEvent(new InGameEvent(t, new FootballGoalPayload { Contestant = red, Minute = 10 }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(20), new FootballGoalPayload { Contestant = blue, Minute = 30 }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(40), new FootballGoalPayload { Contestant = red, Minute = 50 }));

        var ctrl = new FootballMatchController(match);

        Assert.That(ctrl.GetGoalCount(red), Is.EqualTo(2));
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
        match.RecordEvent(new InGameEvent(t, new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 30, Referee = referee }));
        match.RecordEvent(new InGameEvent(t.AddMinutes(30), new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 60, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.True);
    }

    [Test]
    public void IsPlayerSentOff_DirectRedCard_ReturnsTrue()
    {
        var player = T("Blue");
        var match = new Match("Test", new[] { player });
        var referee = R("Referee");
        match.RecordEvent(new InGameEvent(DateTime.Now,
            new FootballCardPayload { Contestant = player, CardType = CardType.Red, Minute = 50, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.True);
    }

    [Test]
    public void IsPlayerSentOff_OneYellowCard_ReturnsFalse()
    {
        var player = T("Blue");
        var match = new Match("Test", new[] { player });
        var referee = R("Referee");
        match.RecordEvent(new InGameEvent(DateTime.Now,
            new FootballCardPayload { Contestant = player, CardType = CardType.Yellow, Minute = 30, Referee = referee }));

        Assert.That(new FootballMatchController(match).IsPlayerSentOff(player), Is.False);
    }

    [Test]
    public void GetSubstitutions_CountsCorrectly()
    {
        var red = T("Red");
        var match = new Match("Test", new[] { red });
        var playerIn = T("Sub");
        var playerOff = T("Starter");
        match.RecordEvent(new InGameEvent(DateTime.Now,
            new FootballSubstitutionPayload { PlayerIn = playerIn, PlayerOff = playerOff, Minute = 65 }));

        var subs = new FootballMatchController(match).GetSubstitutions();

        Assert.That(subs.Count, Is.EqualTo(1));
        Assert.That(subs[0].Minute, Is.EqualTo(65));
        Assert.That(subs[0].PlayerOff, Is.SameAs(playerOff));
    }
}