using System.Collections.ObjectModel;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace SkiJumpingTests;

// ─── SkiJumpingDuelStrategy unit tests ───────────────────────────────────────

[TestFixture]
public class SkiJumpingDuelStrategyTests
{
    [Test]
    public void CreateMatches_EvenContestants_CreatesPairs()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 42));
        var matches = strategy.CreateMatches(H.NumCs(50));

        Assert.That(matches.Count, Is.EqualTo(25));
        Assert.That(matches.Select(m => m.Contestants.Count), Has.All.EqualTo(2));
    }

    [Test]
    public void CreateMatches_AllContestantsAppearExactlyOnce()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 42));
        var contestants = H.NumCs(50);
        var matches = strategy.CreateMatches(contestants);

        var paired = matches.SelectMany(m => m.Contestants).ToList();
        Assert.That(paired.Count, Is.EqualTo(50));
        Assert.That(paired.Distinct().Count(), Is.EqualTo(50));
    }

    [Test]
    public void CreateMatches_OddContestantCount_LastIsDropped()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 0));
        var matches = strategy.CreateMatches(H.NumCs(7));

        // 7 contestants → 3 pairs; 1 left over and skipped
        Assert.That(matches.Count, Is.EqualTo(3));
        Assert.That(matches.SelectMany(m => m.Contestants).Distinct().Count(), Is.EqualTo(6));
    }

    [Test]
    public void CreateMatches_SameSeedProducesIdenticalPairing()
    {
        var contestants = H.NumCs(10);
        var d1 = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 99)).CreateMatches(contestants);
        var d2 = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 99)).CreateMatches(contestants);

        var pairs1 = d1.Select(m => (m.Contestants[0].Name, m.Contestants[1].Name));
        var pairs2 = d2.Select(m => (m.Contestants[0].Name, m.Contestants[1].Name));
        Assert.That(pairs1, Is.EqualTo(pairs2));
    }

    [Test]
    public void CreateNextRound_Returns25WinnersPlus5BestLosers()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 1));
        var duels = strategy.CreateMatches(H.NumCs(50));

        foreach (var duel in duels)
        {
            duel.SetScore(duel.Contestants[0], H.Pts(150f));
            duel.SetScore(duel.Contestants[1], H.Pts(100f));
        }

        var finals = strategy.CreateNextRound(duels);

        Assert.That(finals, Is.Not.Null);
        Assert.That(finals!.Count, Is.EqualTo(1));
        Assert.That(finals[0].Contestants.Count, Is.EqualTo(30));
    }

    [Test]
    public void CreateNextRound_Top5LosersAreCorrectlySelected()
    {
        // 12 contestants → 6 duels; loser in duel i scores (i+1)*10 → worst loser is duel 0's loser
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 1));
        var duels = strategy.CreateMatches(H.NumCs(12));

        var expectedWorstLoser = duels[0].Contestants[1];
        for (int i = 0; i < duels.Count; i++)
        {
            duels[i].SetScore(duels[i].Contestants[0], H.Pts(200f));
            duels[i].SetScore(duels[i].Contestants[1], H.Pts((i + 1) * 10f));
        }

        var finals = strategy.CreateNextRound(duels);

        // 6 winners + 5 best losers = 11; the loser with score 10 (duel 0) should be excluded
        Assert.That(finals![0].Contestants.Count, Is.EqualTo(11));
        Assert.That(finals[0].Contestants, Does.Not.Contain(expectedWorstLoser));
    }

    [Test]
    public void CreateNextRound_WinnersAreHigherScoringContestantsFromEachDuel()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 5));
        var duels = strategy.CreateMatches(H.NumCs(10));

        // First contestant in each duel is the loser; second is the winner
        var expectedWinners = new HashSet<IContestant>();
        foreach (var duel in duels)
        {
            duel.SetScore(duel.Contestants[0], H.Pts(80f));
            duel.SetScore(duel.Contestants[1], H.Pts(120f));
            expectedWinners.Add(duel.Contestants[1]);
        }

        var finals = strategy.CreateNextRound(duels);

        // All expected winners appear in the final
        foreach (var winner in expectedWinners)
            Assert.That(finals![0].Contestants, Contains.Item(winner));
    }

    [Test]
    public void CreateNextRound_SecondCallReturnsNull()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 1));
        var duels = strategy.CreateMatches(H.NumCs(10));

        foreach (var d in duels)
        {
            d.SetScore(d.Contestants[0], H.Pts(100f));
            d.SetScore(d.Contestants[1], H.Pts(50f));
        }

        var finals = strategy.CreateNextRound(duels);
        Assert.That(finals, Is.Not.Null);

        var second = strategy.CreateNextRound(finals!);
        Assert.That(second, Is.Null);
    }

    [Test]
    public void CreateMatches_ResetsFinalFlagSoCreateNextRoundWorksAgain()
    {
        var strategy = new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 2));
        var contestants = H.NumCs(10);

        var duels1 = strategy.CreateMatches(contestants);
        foreach (var d in duels1)
        {
            d.SetScore(d.Contestants[0], H.Pts(100f));
            d.SetScore(d.Contestants[1], H.Pts(50f));
        }
        strategy.CreateNextRound(duels1); // _finalCreated = true

        // Calling CreateMatches again must reset state
        var duels2 = strategy.CreateMatches(contestants);
        foreach (var d in duels2)
        {
            d.SetScore(d.Contestants[0], H.Pts(100f));
            d.SetScore(d.Contestants[1], H.Pts(50f));
        }
        var finals2 = strategy.CreateNextRound(duels2);

        Assert.That(finals2, Is.Not.Null);
    }
}