using System.Collections.ObjectModel;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace tests;

// ─── helpers shared by all fixtures ──────────────────────────────────────────

file static class H
{
    public static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    public static List<IContestant> Cs(params string[] names) =>
        names.Select(C).ToList();

    /// <summary>Creates n contestants named A01…A{n:D2}.</summary>
    public static List<IContestant> NumCs(int n) =>
        Enumerable.Range(1, n).Select(i => C($"A{i:D2}")).ToList();

    public static IScore Pts(float v) => new SkiJumpingScore(v, 0f, 0f, 0f);

    public static int Idx(IContestant c) => int.Parse(c.Name[1..]);
}

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
            duel.Statistics[duel.Contestants[0]] = H.Pts(150f);
            duel.Statistics[duel.Contestants[1]] = H.Pts(100f);
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
            duels[i].Statistics[duels[i].Contestants[0]] = H.Pts(200f);
            duels[i].Statistics[duels[i].Contestants[1]] = H.Pts((i + 1) * 10f);
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
            duel.Statistics[duel.Contestants[0]] = H.Pts(80f);
            duel.Statistics[duel.Contestants[1]] = H.Pts(120f);
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
            d.Statistics[d.Contestants[0]] = H.Pts(100f);
            d.Statistics[d.Contestants[1]] = H.Pts(50f);
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
            d.Statistics[d.Contestants[0]] = H.Pts(100f);
            d.Statistics[d.Contestants[1]] = H.Pts(50f);
        }
        strategy.CreateNextRound(duels1); // _finalCreated = true

        // Calling CreateMatches again must reset state
        var duels2 = strategy.CreateMatches(contestants);
        foreach (var d in duels2)
        {
            d.Statistics[d.Contestants[0]] = H.Pts(100f);
            d.Statistics[d.Contestants[1]] = H.Pts(50f);
        }
        var finals2 = strategy.CreateNextRound(duels2);

        Assert.That(finals2, Is.Not.Null);
    }
}

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
            matches[0].Statistics[c] = H.Pts(H.Idx(c) <= 20 ? 50f : 200f);

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
            matches[0].Statistics[c] = H.Pts(H.Idx(c));

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
        strategy.CreateNextRound(new List<IMatch>()); // sets _finalCreated = true

        var matches = strategy.CreateMatches(H.NumCs(5)); // must reset
        var finals = strategy.CreateNextRound(matches);

        Assert.That(finals, Is.Not.Null);
    }
}

// ─── MultiTournament + FourHills integration tests ───────────────────────────

[TestFixture]
public class FourHillsTournamentIntegrationTests
{
    [Test]
    public void Start_CreatesFourSubTournamentsEachWith25DuelMatches()
    {
        var contestants = H.NumCs(50);
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 7)));
        t.Contestants.AddRange(contestants);

        t.Start();

        Assert.That(t.SubTournaments.Count, Is.EqualTo(4));
        foreach (var sub in t.SubTournaments.Cast<SingleTournament>())
            Assert.That(sub.Matches.Count, Is.EqualTo(25));
    }

    [Test]
    public void End_AggregatesHillResultsIntoTournamentResults()
    {
        var contestants = H.Cs("Alpha", "Beta", "Gamma");
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        t.Contestants.AddRange(contestants);
        t.Start();

        // Alpha: 100+95+95+95=385  Beta: 90+100+100+100=390  Gamma: 80+80+80+80=320
        float[][] perHill =
        {
            new[] { 100f, 90f, 80f },
            new[] { 95f, 100f, 80f },
            new[] { 95f, 100f, 80f },
            new[] { 95f, 100f, 80f },
        };
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
                t.SubTournaments[i].TournamentResults[contestants[j]] = H.Pts(perHill[i][j]);

        t.End();

        Assert.That(t.TournamentResults[contestants[0]].GetValue(), Is.EqualTo(385).Within(0.01));
        Assert.That(t.TournamentResults[contestants[1]].GetValue(), Is.EqualTo(390).Within(0.01));
        Assert.That(t.TournamentResults[contestants[2]].GetValue(), Is.EqualTo(320).Within(0.01));
    }

    [Test]
    public void End_ReportsCorrectOverallWinner()
    {
        var contestants = H.Cs("Alpha", "Beta", "Gamma");
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        t.Contestants.AddRange(contestants);
        t.Start();

        // Beta wins aggregate (390) despite Alpha winning 3 hills
        float[][] perHill =
        {
            new[] { 100f, 90f, 80f },
            new[] { 95f, 100f, 80f },
            new[] { 95f, 100f, 80f },
            new[] { 95f, 100f, 80f },
        };
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
                t.SubTournaments[i].TournamentResults[contestants[j]] = H.Pts(perHill[i][j]);

        t.End();

        var winner = t.TournamentResults.OrderByDescending(kv => kv.Value.GetValue()).First().Key;
        Assert.That(winner.Name, Is.EqualTo("Beta"));
    }

    [Test]
    public void FullSimulation_WinnerHasHighestAggregateScoreAcrossAllFourHills()
    {
        // 50 athletes. Score in every match = index * 10 (A50 always scores highest).
        // A50 wins all duels → makes all 4 finals → highest aggregate.
        var contestants = H.NumCs(50);
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 42)));
        t.Contestants.AddRange(contestants);
        t.Start();

        foreach (var sub in t.SubTournaments.Cast<SingleTournament>())
        {
            // Set duel scores by athlete index
            foreach (var duel in sub.Matches)
            {
                duel.Statistics[duel.Contestants[0]] = H.Pts(H.Idx(duel.Contestants[0]) * 10f);
                duel.Statistics[duel.Contestants[1]] = H.Pts(H.Idx(duel.Contestants[1]) * 10f);
            }

            // Advance to final
            var finalMatches = sub.MatchesStrategy.CreateNextRound(sub.Matches);
            Assert.That(finalMatches, Is.Not.Null, $"{sub.Name}: CreateNextRound returned null");
            sub.Matches.AddRange(finalMatches!);

            // Set final scores and build hill TournamentResults
            var finalMatch = finalMatches![0];
            foreach (var c in finalMatch.Contestants)
            {
                var score = H.Idx(c) * 10f;
                finalMatch.Statistics[c] = H.Pts(score);
                sub.TournamentResults[c] = H.Pts(score);
            }

            sub.End();
        }

        t.End();

        // A50 should have the highest aggregate
        Assert.That(t.TournamentResults, Contains.Key(contestants[49]),
            "A50 should appear in final results");

        var winner = t.TournamentResults.OrderByDescending(kv => kv.Value.GetValue()).First().Key;
        Assert.That(winner.Name, Is.EqualTo("A50"));
    }

    [Test]
    public void FullSimulation_FinalContains30Contestants()
    {
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 0)));
        t.Contestants.AddRange(H.NumCs(50));
        t.Start();

        var firstHill = (SingleTournament)t.SubTournaments[0];
        foreach (var duel in firstHill.Matches)
        {
            duel.Statistics[duel.Contestants[0]] = H.Pts(100f);
            duel.Statistics[duel.Contestants[1]] = H.Pts(80f);
        }

        var finals = firstHill.MatchesStrategy.CreateNextRound(firstHill.Matches);

        Assert.That(finals![0].Contestants.Count, Is.EqualTo(30));
    }
}
