using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

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
        match.Statistics[red] = new FootballMatchScore { GoalsScored = 3 };
        match.Statistics[blue] = new FootballMatchScore { GoalsScored = 1 };

        Assert.That(match.GetWinner(), Is.SameAs(red));
    }

    [Test]
    public void GetWinner_TiedScore_ReturnsPenaltyWinner()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        match.Statistics[red] = new FootballMatchScore { GoalsScored = 2 };
        match.Statistics[blue] = new FootballMatchScore { GoalsScored = 2 };
        match.PenaltyWinner = blue;

        Assert.That(match.GetWinner(), Is.SameAs(blue));
    }

    [Test]
    public void GetWinner_TiedWithNoPenaltyWinner_ReturnsNull()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue });
        match.Statistics[red] = new FootballMatchScore { GoalsScored = 1 };
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