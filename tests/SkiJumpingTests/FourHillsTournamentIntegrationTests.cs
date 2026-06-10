using SkiJumpingTests;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;
using System.Collections.ObjectModel;

namespace SkiJumpingTests;

// ─── MultiTournament + FourHills integration tests ───────────────────────────

[TestFixture]
public class FourHillsTournamentIntegrationTests
{
    [Test]
    public void Start_CreatesFourSubTournamentsEachWith25DuelMatches()
    {
        var contestants = H.NumCs(50);
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 7)), contestants);

        t.Start();

        Assert.That(t.SubTournaments.Count, Is.EqualTo(4));
        foreach (var sub in t.SubTournaments.Cast<SingleTournament>())
            Assert.That(sub.Matches.Count, Is.EqualTo(25));
    }

    [Test]
    public void End_AggregatesHillResultsIntoTournamentResults()
    {
        var contestants = H.Cs("Alpha", "Beta", "Gamma");
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()), contestants);
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
                t.SubTournaments[i].SetResult(contestants[j], H.Pts(perHill[i][j]));

        t.End();

        Assert.That(t.TournamentResults[contestants[0]].GetValue(), Is.EqualTo(385).Within(0.01));
        Assert.That(t.TournamentResults[contestants[1]].GetValue(), Is.EqualTo(390).Within(0.01));
        Assert.That(t.TournamentResults[contestants[2]].GetValue(), Is.EqualTo(320).Within(0.01));
    }

    [Test]
    public void End_ReportsCorrectOverallWinner()
    {
        var contestants = H.Cs("Alpha", "Beta", "Gamma");
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()), contestants);
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
                t.SubTournaments[i].SetResult(contestants[j], H.Pts(perHill[i][j]));

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
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 42)), contestants);
        t.Start();

        foreach (var sub in t.SubTournaments.Cast<SingleTournament>())
        {
            // Set duel scores by athlete index
            foreach (var duel in sub.Matches)
            {
                duel.SetScore(duel.Contestants[0], H.Pts(H.Idx(duel.Contestants[0]) * 10f));
                duel.SetScore(duel.Contestants[1], H.Pts(H.Idx(duel.Contestants[1]) * 10f));
            }

            // Advance to final
            var finalMatches = sub.AdvanceRound();
            Assert.That(finalMatches, Is.Not.Empty, $"{sub.Name}: AdvanceRound returned no matches");

            // Set final scores and build hill TournamentResults
            var finalMatch = finalMatches[0];
            foreach (var c in finalMatch.Contestants)
            {
                var score = H.Idx(c) * 10f;
                finalMatch.SetScore(c, H.Pts(score));
                sub.SetResult(c, H.Pts(score));
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
        var t = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 0)), H.NumCs(50));
        t.Start();

        var firstHill = (SingleTournament)t.SubTournaments[0];
        foreach (var duel in firstHill.Matches)
        {
            duel.SetScore(duel.Contestants[0], H.Pts(100f));
            duel.SetScore(duel.Contestants[1], H.Pts(80f));
        }

        var finals = firstHill.MatchesStrategy.CreateNextRound(firstHill.Matches);

        Assert.That(finals![0].Contestants.Count, Is.EqualTo(30));
    }
}
