using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;

// ─── Score types ──────────────────────────────────────────────────────────────

[TestFixture]
public class FootballMatchScoreTests
{
    [Test]
    public void GetValue_ReturnsGoalsScored()
    {
        var score = new FootballMatchScore(goalsScored: 3);
        Assert.That(score.GetValue(), Is.EqualTo(3));
    }

    [Test]
    public void GetValue_Zero_WhenNoGoals()
    {
        var score = new FootballMatchScore();
        Assert.That(score.GetValue(), Is.EqualTo(0));
    }

    [Test]
    public void AddGoal_Increments()
    {
        var score = new FootballMatchScore();
        score.AddGoal();
        score.AddGoal();
        Assert.That(score.GoalsScored, Is.EqualTo(2));
    }
}
