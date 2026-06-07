using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;

[TestFixture]
public class FootballLeaderboardScoreTests
{
    [Test]
    public void GetValue_ReturnsPoints()
    {
        var score = new FootballLeaderboardScore { Points = 9 };
        Assert.That(score.GetValue(), Is.EqualTo(9));
    }

    [Test]
    public void GoalDifference_ScoredMinusConceded()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 7, GoalsConceded = 3 };
        Assert.That(score.GoalDifference, Is.EqualTo(4));
    }

    [Test]
    public void GoalDifference_NegativeWhenConcededMore()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 1, GoalsConceded = 4 };
        Assert.That(score.GoalDifference, Is.EqualTo(-3));
    }

    [Test]
    public void GoalDifference_ZeroWhenEqual()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 2, GoalsConceded = 2 };
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
        // Sanity: 3 wins + 2 draws + 1 loss = 11 points
        var score = new FootballLeaderboardScore
        {
            Wins = 3,
            Draws = 2,
            Losses = 1,
            Points = 3 * 3 + 2 * 1 + 1 * 0,
        };
        Assert.That(score.GetValue(), Is.EqualTo(11));
    }

    [Test]
    public void GroupRanking_AwardsThreePointsForWinOneForDraw()
    {
        // Red 2-0 Blue (win), Red 1-1 Green (draw), Blue 0-0 Green (draw) → Red:4, Green:2, Blue:1
        var red = new FootballLeaderboardScore { Wins = 1, Draws = 1, Losses = 0, Points = 4 };
        var green = new FootballLeaderboardScore { Wins = 0, Draws = 2, Losses = 0, Points = 2 };
        var blue = new FootballLeaderboardScore { Wins = 0, Draws = 1, Losses = 1, Points = 1 };

        Assert.That(red.GetValue(), Is.EqualTo(4));
        Assert.That(green.GetValue(), Is.EqualTo(2));
        Assert.That(blue.GetValue(), Is.EqualTo(1));
    }
}