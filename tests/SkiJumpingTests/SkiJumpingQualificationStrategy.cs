using System.Collections.ObjectModel;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace SkiJumpingTests;

// ─── SkiJumpingQualificationStrategy unit tests ───────────────────────────────

[TestFixture]
public class SkiJumpingQualificationStrategyTests
{
    [Test]
    public void CreateMatches_AllContestantsInOneQualificationMatch()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        var contestants = H.NumCs(50);

        var matches = strategy.CreateMatches(contestants);

        Assert.That(matches.Count, Is.EqualTo(1));
        Assert.That(matches[0].Name, Is.EqualTo("Qualification"));
        Assert.That(matches[0].Contestants, Is.EquivalentTo(contestants));
    }

    [Test]
    public void CreateNextRound_Top30AdvanceToFinals()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        var contestants = H.NumCs(50);
        var matches = strategy.CreateMatches(contestants);

        // A01–A20 score low (50), A21–A50 score high (200)
        foreach (var c in contestants)
            matches[0].SetScore(c, H.Pts(H.Idx(c) <= 20 ? 50f : 200f));

        var finals = strategy.CreateNextRound(matches);

        Assert.That(finals!.Count, Is.EqualTo(1));
        Assert.That(finals[0].Name, Is.EqualTo("Finals"));
        Assert.That(finals[0].Contestants.Count, Is.EqualTo(30));

        // All finalists should be from the high-scoring group (A21–A50)
        var finalistNames = finals[0].Contestants.Select(c => c.Name).ToHashSet();
        for (int i = 21; i <= 50; i++)
            Assert.That(finalistNames, Contains.Item($"A{i:D2}"));
    }

    [Test]
    public void CreateNextRound_ExactlyTop30AdvanceWhenScoresAreDistinct()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        var contestants = H.NumCs(50);
        var matches = strategy.CreateMatches(contestants);

        // Each athlete has a unique score equal to their index
        foreach (var c in contestants)
            matches[0].SetScore(c, H.Pts(H.Idx(c)));

        var finals = strategy.CreateNextRound(matches);

        Assert.That(finals![0].Contestants.Count, Is.EqualTo(30));

        // A21–A50 have the 30 highest scores
        var finalistIndices = finals[0].Contestants.Select(H.Idx).ToHashSet();
        Assert.That(finalistIndices.Min(), Is.EqualTo(21));
        Assert.That(finalistIndices.Max(), Is.EqualTo(50));
    }

    [Test]
    public void CreateNextRound_FewerThan30Contestants_AllAdvance()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        var contestants = H.NumCs(20);
        var matches = strategy.CreateMatches(contestants);

        var finals = strategy.CreateNextRound(matches);

        Assert.That(finals![0].Contestants.Count, Is.EqualTo(20));
    }

    [Test]
    public void CreateNextRound_SecondCallReturnsNull()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        var matches = strategy.CreateMatches(H.NumCs(10));

        strategy.CreateNextRound(matches);
        var second = strategy.CreateNextRound(matches);

        Assert.That(second, Is.Null);
    }

    [Test]
    public void CreateMatches_ResetsFinalFlagSoCreateNextRoundWorksAgain()
    {
        var strategy = new SkiJumpingQualificationStrategy();
        strategy.CreateMatches(H.NumCs(5));
        strategy.CreateNextRound(new List<Match>()); // sets _finalCreated = true

        var matches = strategy.CreateMatches(H.NumCs(5)); // must reset
        var finals = strategy.CreateNextRound(matches);

        Assert.That(finals, Is.Not.Null);
    }
}
