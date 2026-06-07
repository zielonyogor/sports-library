using System.Collections.ObjectModel;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace SkiJumpingTests;

// ─── FourHillsStrategy unit tests ────────────────────────────────────────────

[TestFixture]
public class FourHillsStrategyTests
{
    private static readonly string[] ExpectedNames =
    {
        "Oberstdorf Tournament",
        "Garmisch-Partenkirchen Tournament",
        "Innsbruck Tournament",
        "Bischofshofen Tournament",
    };

    [Test]
    public void CreateSubTournaments_CreatesFourHillsInOrder()
    {
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var hills = strategy.CreateSubTournaments(H.Cs("A", "B"));

        Assert.That(hills.Count, Is.EqualTo(4));
        Assert.That(hills.Select(h => h.Name), Is.EqualTo(ExpectedNames));
    }

    [Test]
    public void CreateSubTournaments_EachHillContainsAllContestants()
    {
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var contestants = H.Cs("A", "B", "C");
        var hills = strategy.CreateSubTournaments(contestants);

        foreach (var hill in hills)
            Assert.That(hill.Contestants, Is.EqualTo(contestants));
    }

    [Test]
    public void CreateNextStage_AlwaysReturnsNull()
    {
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        Assert.That(strategy.CreateNextStage(new List<ITournament>()), Is.Null);
    }

    [Test]
    public void AggregateResults_SumsScoresAcrossAllFourHills()
    {
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var contestants = H.Cs("Kamil", "Dawid");
        var kamil = contestants[0];
        var dawid = contestants[1];
        var hills = strategy.CreateSubTournaments(contestants);

        // Kamil 100+90+90+90=370, Dawid 90+110+100+100=400
        float[] kPts = { 100, 90, 90, 90 };
        float[] dPts = { 90, 110, 100, 100 };
        for (int i = 0; i < 4; i++)
        {
            hills[i].TournamentResults[kamil] = H.Pts(kPts[i]);
            hills[i].TournamentResults[dawid] = H.Pts(dPts[i]);
        }

        var results = strategy.AggregateResults(hills);

        Assert.That(results[kamil].GetValue(), Is.EqualTo(370).Within(0.01));
        Assert.That(results[dawid].GetValue(), Is.EqualTo(400).Within(0.01));
    }

    [Test]
    public void AggregateResults_ContestantWinningMostHillsCanStillLoseAggregate()
    {
        // Stefan wins 3 hills narrowly; Thomas wins only Bischofshofen by a big margin.
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var contestants = H.Cs("Stefan", "Thomas");
        var stefan = contestants[0];
        var thomas = contestants[1];
        var hills = strategy.CreateSubTournaments(contestants);

        // Stefan: 110+110+110+10=340  Thomas: 100+100+100+200=500
        float[] sPts = { 110, 110, 110, 10 };
        float[] tPts = { 100, 100, 100, 200 };
        for (int i = 0; i < 4; i++)
        {
            hills[i].TournamentResults[stefan] = H.Pts(sPts[i]);
            hills[i].TournamentResults[thomas] = H.Pts(tPts[i]);
        }

        var results = strategy.AggregateResults(hills);

        Assert.That(results[thomas].GetValue(), Is.EqualTo(500).Within(0.01));
        Assert.That(results[stefan].GetValue(), Is.EqualTo(340).Within(0.01));
        Assert.That(results[thomas].GetValue(), Is.GreaterThan(results[stefan].GetValue()));
    }

    [Test]
    public void AggregateResults_UsesTotalAcrossRoundsToSelectWinner()
    {
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var contestants = H.Cs("Kamil", "Dawid", "Stefan");
        var kamil = contestants[0]; var dawid = contestants[1]; var stefan = contestants[2];
        var hills = strategy.CreateSubTournaments(contestants);

        // Round 1: Kamil 181.4, Dawid 181.2, Stefan 179.2
        hills[0].TournamentResults[kamil] = new SkiJumpingScore(126.5f, 56.0f, -2.1f, 1.0f);
        hills[0].TournamentResults[dawid] = new SkiJumpingScore(125.0f, 55.5f, 0.4f, 0.3f);
        hills[0].TournamentResults[stefan] = new SkiJumpingScore(124.0f, 55.0f, 0.2f, 0.0f);
        // Round 2: Kamil 178.7, Dawid 185.2, Stefan 178.1 → Dawid wins aggregate (366.4 vs 360.1)
        hills[1].TournamentResults[kamil] = new SkiJumpingScore(125.0f, 55.0f, -2.0f, 0.7f);
        hills[1].TournamentResults[dawid] = new SkiJumpingScore(127.5f, 56.0f, 1.2f, 0.5f);
        hills[1].TournamentResults[stefan] = new SkiJumpingScore(123.5f, 54.5f, 0.1f, 0.0f);

        var results = strategy.AggregateResults(new List<ITournament> { hills[0], hills[1] });
        var winner = results.OrderByDescending(kv => kv.Value.GetValue()).First().Key;
        Assert.That(winner.Name, Is.EqualTo("Dawid"));
    }

    [Test]
    public void AggregateResults_PartialParticipation_OnlyCompletedHillsCountedInTotal()
    {
        // Peter completes all 4 hills; Andreas skips Innsbruck (no result entry).
        var strategy = new FourHillsStrategy(new DefaultRandomProvider());
        var contestants = H.Cs("Peter", "Andreas");
        var peter = contestants[0];
        var andreas = contestants[1];
        var hills = strategy.CreateSubTournaments(contestants);

        for (int i = 0; i < 4; i++)
            hills[i].TournamentResults[peter] = H.Pts(100f);

        // Andreas has no result for hill 2 (index 2 = Innsbruck)
        hills[0].TournamentResults[andreas] = H.Pts(200f);
        hills[1].TournamentResults[andreas] = H.Pts(200f);
        hills[3].TournamentResults[andreas] = H.Pts(200f);

        var results = strategy.AggregateResults(hills);

        Assert.That(results[peter].GetValue(), Is.EqualTo(400).Within(0.01));
        Assert.That(results[andreas].GetValue(), Is.EqualTo(600).Within(0.01));
    }
}