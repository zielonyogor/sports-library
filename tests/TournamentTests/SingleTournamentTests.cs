using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace TimelineTests;

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