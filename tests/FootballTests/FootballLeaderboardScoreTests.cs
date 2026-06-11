using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;

[TestFixture]
public class FootballLeaderboardScoreTests
{
    [Test]
    public void GetValue_ReturnsPoints()
    {
        var score = new FootballLeaderboardScore(wins: 3, draws: 0, losses: 0);
        Assert.That(score.GetValue(), Is.EqualTo(9));
    }

    [Test]
    public void GoalDifference_ScoredMinusConceded()
    {
        var score = new FootballLeaderboardScore(wins: 0, draws: 0, losses: 0, goalsScored: 7, goalsConceded: 3);
        Assert.That(score.GoalDifference, Is.EqualTo(4));
    }

    [Test]
    public void GoalDifference_NegativeWhenConcededMore()
    {
        var score = new FootballLeaderboardScore(wins: 0, draws: 0, losses: 0, goalsScored: 1, goalsConceded: 4);
        Assert.That(score.GoalDifference, Is.EqualTo(-3));
    }

    [Test]
    public void GoalDifference_ZeroWhenEqual()
    {
        var score = new FootballLeaderboardScore(wins: 0, draws: 0, losses: 0, goalsScored: 2, goalsConceded: 2);
        Assert.That(score.GoalDifference, Is.EqualTo(0));
    }

    [Test]
    public void WinsDrawsLosses_DefaultZero()
    {
        var score = new FootballLeaderboardScore();
        Assert.That(score.Wins, Is.EqualTo(0));
        Assert.That(score.Draws, Is.EqualTo(0));
        Assert.That(score.Losses, Is.EqualTo(0));
    }

    [Test]
    public void Points_StandardLeague_ThreeForWinOneForDraw()
    {
        // 3 wins + 2 draws + 1 loss → 3*3 + 2*1 + 0 = 11 points
        var score = new FootballLeaderboardScore();
        for (int i = 0; i < 3; i++) score.RegisterWin();
        for (int i = 0; i < 2; i++) score.RegisterDraw();
        score.RegisterLoss();
        Assert.That(score.GetValue(), Is.EqualTo(11));
    }

    [Test]
    public void GroupRanking_AwardsThreePointsForWinOneForDraw()
    {
        // Red 2-0 Blue (win), Red 1-1 Green (draw), Blue 0-0 Green (draw)
        // → Red: 1W+1D=4, Green: 2D=2, Blue: 1D+1L=1
        var red = new FootballLeaderboardScore();
        red.RegisterWin(goalsFor: 2, goalsAgainst: 0);
        red.RegisterDraw(goalsFor: 1, goalsAgainst: 1);

        var green = new FootballLeaderboardScore();
        green.RegisterDraw(goalsFor: 1, goalsAgainst: 1);
        green.RegisterDraw(goalsFor: 0, goalsAgainst: 0);

        var blue = new FootballLeaderboardScore();
        blue.RegisterLoss(goalsFor: 0, goalsAgainst: 2);
        blue.RegisterDraw(goalsFor: 0, goalsAgainst: 0);

        Assert.That(red.GetValue(), Is.EqualTo(4));
        Assert.That(green.GetValue(), Is.EqualTo(2));
        Assert.That(blue.GetValue(), Is.EqualTo(1));
    }
}
