using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace PayloadTests;

// ─── SkiJumpingScore ──────────────────────────────────────────────────────────

[TestFixture]
public class SkiJumpingScoreTests
{
    [Test]
    public void Points_SumsAllFourComponents()
    {
        var score = new SkiJumpingScore(130f, 57f, -2f, 1.2f);
        Assert.That(score.Points, Is.EqualTo(186.2f).Within(0.001f));
    }

    [Test]
    public void GetValue_ReturnsPoints()
    {
        var score = new SkiJumpingScore(130f, 57f, 0f, 0f);
        Assert.That(score.GetValue(), Is.EqualTo(187.0).Within(0.001));
    }

    [Test]
    public void WindCompensation_CanBeNegative_ReducesTotalPoints()
    {
        var withoutWind = new SkiJumpingScore(130f, 57f, 0f, 0f);
        var withHeadwind = new SkiJumpingScore(130f, 57f, -3.6f, 0f);
        Assert.That(withHeadwind.Points, Is.LessThan(withoutWind.Points));
    }

    [Test]
    public void GateCompensation_CanBePositive_IncreasesTotalPoints()
    {
        var withoutGate = new SkiJumpingScore(130f, 57f, 0f, 0f);
        var withGate = new SkiJumpingScore(130f, 57f, 0f, 7.2f);
        Assert.That(withGate.Points, Is.GreaterThan(withoutGate.Points));
    }

    [Test]
    public void AllZeroComponents_PointsAreZero()
    {
        var score = new SkiJumpingScore(0f, 0f, 0f, 0f);
        Assert.That(score.Points, Is.EqualTo(0f));
        Assert.That(score.GetValue(), Is.EqualTo(0.0));
    }

    [Test]
    public void Properties_AreSettableAfterConstruction()
    {
        var score = new SkiJumpingScore(0f, 0f, 0f, 0f);
        score.DistancePoints = 130f;
        score.StylePoints = 57f;
        Assert.That(score.Points, Is.EqualTo(187f).Within(0.001f));
    }

    [Test]
    public void TwoScores_HigherPointsWins()
    {
        var a = new SkiJumpingScore(135f, 58f, 1f, 0f);   // 194
        var b = new SkiJumpingScore(130f, 57f, -1f, 0f);  // 186
        Assert.That(a.GetValue(), Is.GreaterThan(b.GetValue()));
    }
}