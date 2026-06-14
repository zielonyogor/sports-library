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

    private static SingleTournament MakeTournament(string name, IMatchesStrategy strategy, IEnumerable<IContestant>? contestants = null, IRankingStrategy? rankingStrategy = null) =>
        new SingleTournament(name, strategy, contestants ?? Enumerable.Empty<IContestant>(), rankingStrategy ?? new DescendingScoreRankingStrategy());

    private static SingleTournament MakeRankedTournament(string name, IMatchesStrategy strategy, IEnumerable<IContestant>? contestants = null) =>
        new SingleTournament(name, strategy, contestants ?? Enumerable.Empty<IContestant>(), new FootballGroupRankingStrategy());

    [Test]
    public void Id_UniquePerInstance()
    {
        var t1 = MakeTournament("T", new FootballGroupStageStrategy());
        var t2 = MakeTournament("T", new FootballGroupStageStrategy());
        Assert.That(t1.Id, Is.Not.EqualTo(t2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var t = MakeTournament("Group A", new FootballGroupStageStrategy());
        Assert.That(t.Name, Is.EqualTo("Group A"));
    }

    [Test]
    public void Matches_EmptyBeforeStart()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.Matches, Is.Empty);
    }

    [Test]
    public void Contestants_EmptyByDefault()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.Contestants, Is.Empty);
    }

    [Test]
    public void TournamentResults_EmptyByDefault()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy());
        Assert.That(t.TournamentResults, Is.Empty);
    }

    [Test]
    public void Start_PopulatesMatchesViaStrategy()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy(), Teams("A", "B", "C", "D"));
        t.Start();
        Assert.That(t.Matches, Is.Not.Empty);
    }

    [Test]
    public void Start_MatchCountMatchesRoundRobinFormula()
    {
        // 4 teams → 4*3/2 = 6 matches
        var t = MakeTournament("Group A", new FootballGroupStageStrategy(), Teams("A", "B", "C", "D"));
        t.Start();
        Assert.That(t.Matches.Count, Is.EqualTo(6));
    }

    [Test]
    public void Start_WithSkiJumpingDuelStrategy_Creates25Matches()
    {
        var t = MakeTournament("Hill",
            new SkiJumpingDuelStrategy(new DefaultRandomProvider(seed: 0)),
            Enumerable.Range(1, 50).Select(i => T($"A{i}")));
        t.Start();
        Assert.That(t.Matches.Count, Is.EqualTo(25));
    }

    [Test]
    public void End_WithDefaultRankingStrategy_DoesNotThrow()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy(), Teams("A", "B"));
        t.Start();
        Assert.DoesNotThrow(() => t.End());
    }

    [Test]
    public void End_WithRankingStrategy_RanksResults()
    {
        var teams = Teams("A", "B", "C");
        var t = MakeRankedTournament("T", new FootballGroupStageStrategy(), teams);
        t.Start();
        t.SetResult(teams[0], new FootballLeaderboardScore(wins: 1, draws: 0, losses: 0, goalsScored: 1, goalsConceded: 0));
        t.SetResult(teams[1], new FootballLeaderboardScore(wins: 2, draws: 0, losses: 0, goalsScored: 4, goalsConceded: 1));
        t.SetResult(teams[2], new FootballLeaderboardScore(wins: 0, draws: 1, losses: 1, goalsScored: 1, goalsConceded: 3));

        t.End();

        Assert.That(t.TournamentResults.Keys.First(), Is.SameAs(teams[1]));
    }

    [Test]
    public void Advance_CanBeDrivenThroughSharedInterface()
    {
        var t = new SingleTournament(
            "T",
            new SkiJumpingQualificationStrategy(),
            Enumerable.Range(1, 50).Select(i => T($"A{i}")),
            new DescendingScoreRankingStrategy());

        t.Start();

        Assert.That(t.Advance(), Is.True);
    }

    [Test]
    public void TournamentResults_CanBePopulatedAndRead()
    {
        var t = MakeTournament("T", new FootballGroupStageStrategy());
        var team = T("Team A");
        t.SetResult(team, new FootballLeaderboardScore(wins: 3, draws: 0, losses: 0));
        Assert.That(t.TournamentResults[team].GetValue(), Is.EqualTo(9));
    }
}
