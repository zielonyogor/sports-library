using SportsLibrary.FootballClasses;
using SportsLibrary.Model;
using SportsLibrary.SkiJumpingClasses;

namespace tests;

// ─── SingleTournament lifecycle ───────────────────────────────────────────────

[TestFixture]
public class SingleTournamentTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    private static List<IContestant> Teams(params string[] names) =>
        names.Select(T).ToList<IContestant>();

    [Test]
    public void Id_UniquePerInstance()
    {
        var t1 = new SingleTournament("T", new FootballGroupStageStrategy());
        var t2 = new SingleTournament("T", new FootballGroupStageStrategy());
        Assert.That(t1.Id, Is.Not.EqualTo(t2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var t = new SingleTournament("Group A", new FootballGroupStageStrategy());
        Assert.That(t.Name, Is.EqualTo("Group A"));
    }

    [Test]
    public void Matches_EmptyBeforeStart()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.Matches, Is.Empty);
    }

    [Test]
    public void Contestants_EmptyByDefault()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.Contestants, Is.Empty);
    }

    [Test]
    public void TournamentResults_EmptyByDefault()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.TournamentResults, Is.Empty);
    }

    [Test]
    public void Start_PopulatesMatchesViaStrategy()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        t.Contestants.AddRange(Teams("A", "B", "C", "D"));
        t.Start();
        Assert.That(t.Matches, Is.Not.Empty);
    }

    [Test]
    public void Start_MatchCountMatchesRoundRobinFormula()
    {
        // 4 teams → 4*3/2 = 6 matches
        var t = new SingleTournament("Group A", new FootballGroupStageStrategy());
        t.Contestants.AddRange(Teams("A", "B", "C", "D"));
        t.Start();
        Assert.That(t.Matches.Count, Is.EqualTo(6));
    }

    [Test]
    public void Start_WithSkiJumpingDuelStrategy_Creates25Matches()
    {
        var t = new SingleTournament("Hill", new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 0)));
        t.Contestants.AddRange(Enumerable.Range(1, 50).Select(i => T($"A{i}")));
        t.Start();
        Assert.That(t.Matches.Count, Is.EqualTo(25));
    }

    [Test]
    public void End_DoesNotThrow()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        t.Contestants.AddRange(Teams("A", "B"));
        t.Start();
        Assert.DoesNotThrow(() => t.End());
    }

    [Test]
    public void TournamentResults_CanBePopulatedExternallyAndRead()
    {
        var t = new SingleTournament("T", new FootballGroupStageStrategy());
        var team = T("Team A");
        t.TournamentResults[team] = new FootballLeaderboardScore { Points = 9 };
        Assert.That(t.TournamentResults[team].GetValue(), Is.EqualTo(9));
    }
}

// ─── MultiTournament lifecycle ────────────────────────────────────────────────

[TestFixture]
public class MultiTournamentTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    private static List<IContestant> Teams(int n) =>
        Enumerable.Range(0, n).Select(i => T($"T{i:D2}")).ToList();

    [Test]
    public void Id_UniquePerInstance()
    {
        var m1 = new MultiTournament("MT", new FourHillsStrategy(new DefaultRandomProvider()));
        var m2 = new MultiTournament("MT", new FourHillsStrategy(new DefaultRandomProvider()));
        Assert.That(m1.Id, Is.Not.EqualTo(m2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var mt = new MultiTournament("Four Hills 2024", new FourHillsStrategy(new DefaultRandomProvider()));
        Assert.That(mt.Name, Is.EqualTo("Four Hills 2024"));
    }

    [Test]
    public void SubTournaments_EmptyBeforeStart()
    {
        var mt = new MultiTournament("MT", new FootballWorldCupStrategy());
        Assert.That(mt.SubTournaments, Is.Empty);
    }

    [Test]
    public void TournamentResults_EmptyBeforeEnd()
    {
        var mt = new MultiTournament("MT", new FourHillsStrategy(new DefaultRandomProvider()));
        mt.Contestants.AddRange(Teams(4));
        mt.Start();
        Assert.That(mt.TournamentResults, Is.Empty);
    }

    [Test]
    public void Start_FourHills_Creates4SubTournaments()
    {
        var mt = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider(seed: 1)));
        mt.Contestants.AddRange(Teams(50));
        mt.Start();
        Assert.That(mt.SubTournaments.Count, Is.EqualTo(4));
    }

    [Test]
    public void Start_WorldCup_Creates8GroupSubTournaments()
    {
        var mt = new MultiTournament("World Cup", new FootballWorldCupStrategy());
        mt.Contestants.AddRange(Teams(32));
        mt.Start();
        Assert.That(mt.SubTournaments.Count, Is.EqualTo(8));
    }

    [Test]
    public void Start_EachSubTournamentIsStarted_HasMatches()
    {
        var mt = new MultiTournament("World Cup", new FootballWorldCupStrategy());
        mt.Contestants.AddRange(Teams(32));
        mt.Start();

        foreach (var sub in mt.SubTournaments.Cast<SingleTournament>())
            Assert.That(sub.Matches, Is.Not.Empty, $"{sub.Name} should have matches after Start");
    }

    [Test]
    public void AdvanceToNextStage_AddsNewSubTournaments()
    {
        var mt = new MultiTournament("World Cup", new FootballWorldCupStrategy());
        mt.Contestants.AddRange(Teams(32));
        mt.Start();

        foreach (var sub in mt.SubTournaments)
            for (int j = 0; j < sub.Contestants.Count; j++)
                sub.TournamentResults[sub.Contestants[j]] =
                    new FootballLeaderboardScore { Points = (sub.Contestants.Count - j) * 3 };

        int before = mt.SubTournaments.Count;
        mt.AdvanceToNextStage();

        Assert.That(mt.SubTournaments.Count, Is.GreaterThan(before));
    }

    [Test]
    public void AdvanceToNextStage_FourHills_StrategyReturnsNull_CountUnchanged()
    {
        // FourHillsStrategy.CreateNextStage always returns null
        var mt = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        mt.Contestants.AddRange(Teams(4));
        mt.Start();

        int before = mt.SubTournaments.Count;
        mt.AdvanceToNextStage();

        Assert.That(mt.SubTournaments.Count, Is.EqualTo(before));
    }

    [Test]
    public void End_PopulatesTournamentResults()
    {
        var mt = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        var teams = Teams(4);
        mt.Contestants.AddRange(teams);
        mt.Start();

        foreach (var sub in mt.SubTournaments)
            foreach (var c in teams)
                sub.TournamentResults[c] = new SkiJumpingScore(100f, 0f, 0f, 0f);

        mt.End();

        Assert.That(mt.TournamentResults, Is.Not.Empty);
        Assert.That(mt.TournamentResults.Count, Is.EqualTo(teams.Count));
    }

    [Test]
    public void End_AggregatesScoresFromAllSubTournaments()
    {
        var mt = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        var teams = Teams(2);
        var t0 = teams[0]; var t1 = teams[1];
        mt.Contestants.AddRange(teams);
        mt.Start();

        // t0: 200 per hill = 800. t1: 100 per hill = 400.
        foreach (var sub in mt.SubTournaments)
        {
            sub.TournamentResults[t0] = new SkiJumpingScore(200f, 0f, 0f, 0f);
            sub.TournamentResults[t1] = new SkiJumpingScore(100f, 0f, 0f, 0f);
        }

        mt.End();

        Assert.That(mt.TournamentResults[t0].GetValue(), Is.EqualTo(800).Within(0.01));
        Assert.That(mt.TournamentResults[t1].GetValue(), Is.EqualTo(400).Within(0.01));
    }

    [Test]
    public void End_WinnerHasHighestAggregateScore()
    {
        var mt = new MultiTournament("Four Hills", new FourHillsStrategy(new DefaultRandomProvider()));
        var teams = Teams(3);
        mt.Contestants.AddRange(teams);
        mt.Start();

        float[] perHill = { 300f, 200f, 100f }; // teams[0] always scores most
        foreach (var sub in mt.SubTournaments)
            for (int i = 0; i < teams.Count; i++)
                sub.TournamentResults[teams[i]] = new SkiJumpingScore(perHill[i], 0f, 0f, 0f);

        mt.End();

        var winner = mt.TournamentResults.OrderByDescending(kv => kv.Value.GetValue()).First().Key;
        Assert.That(winner.Name, Is.EqualTo(teams[0].Name));
    }
}
