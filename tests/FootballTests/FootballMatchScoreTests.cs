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
        var score = new FootballMatchScore { GoalsScored = 3 };
        Assert.That(score.GetValue(), Is.EqualTo(3));
    }

    [Test]
    public void GetValue_Zero_WhenNoGoals()
    {
        var score = new FootballMatchScore { GoalsScored = 0 };
        Assert.That(score.GetValue(), Is.EqualTo(0));
    }

    [Test]
    public void Cards_EmptyByDefault()
    {
        var score = new FootballMatchScore();
        Assert.That(score.Cards, Is.Empty);
    }

    [Test]
    public void Cards_CanStoreMultipleCardTypes()
    {
        var score = new FootballMatchScore();
        score.Cards["Player1"] = CardType.Yellow;
        score.Cards["Player2"] = CardType.Red;

        Assert.That(score.Cards["Player1"], Is.EqualTo(CardType.Yellow));
        Assert.That(score.Cards["Player2"], Is.EqualTo(CardType.Red));
    }

    [Test]
    public void Result_CanBeSetToAllOutcomes()
    {
        foreach (var outcome in Enum.GetValues<MatchOutcome>())
        {
            var score = new FootballMatchScore { Result = outcome };
            Assert.That(score.Result, Is.EqualTo(outcome));
        }
    }
}