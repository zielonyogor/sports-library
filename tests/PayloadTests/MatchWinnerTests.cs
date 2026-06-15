using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

// ─── Match: GetWinner + GetCurrentScore ───────────────────────────────────────

[TestFixture]
public class MatchWinnerTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    private sealed class FixedWinnerStrategy(IContestant winner) : IMatchResultStrategy
    {
        public IContestant? DetermineWinner(Match match) => winner;
    }

    private static Match StartedMatch(params IContestant[] contestants)
    {
        var match = new Match("Test", contestants);
        match.Start();
        return match;
    }

    [Test]
    public void GetWinner_ReturnsHighestScoringContestant()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = StartedMatch(red, blue);
        match.SetScore(red, new FootballMatchScore(goalsScored: 3));
        match.SetScore(blue, new FootballMatchScore(goalsScored: 1));

        Assert.That(match.GetWinner(), Is.SameAs(red));
    }

    [Test]
    public void GetWinner_TiedScore_ReturnsPenaltyWinner()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = StartedMatch(red, blue);
        match.SetScore(red, new FootballMatchScore(goalsScored: 2));
        match.SetScore(blue, new FootballMatchScore(goalsScored: 2));
        match.AssignPenaltyWinner(blue);

        Assert.That(match.GetWinner(), Is.SameAs(blue));
    }

    [Test]
    public void GetWinner_TiedWithNoPenaltyWinner_ReturnsNull()
    {
        var red = T("Red"); var blue = T("Blue");
        var match = StartedMatch(red, blue);
        match.SetScore(red, new FootballMatchScore(goalsScored: 1));
        match.SetScore(blue, new FootballMatchScore(goalsScored: 1));

        Assert.That(match.GetWinner(), Is.Null);
    }

    [Test]
    public void GetWinner_UsesInjectedResultStrategy()
    {
        var red = T("Red");
        var blue = T("Blue");
        var match = new Match("Test", new[] { red, blue }, new FixedWinnerStrategy(blue));

        Assert.That(match.GetWinner(), Is.SameAs(blue));
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
        var match = StartedMatch(team);
        var score = new FootballMatchScore(goalsScored: 2);
        match.SetScore(team, score);

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
