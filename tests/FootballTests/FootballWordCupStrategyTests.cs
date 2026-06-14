using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;


// ─── FootballWorldCupStrategy ─────────────────────────────────────────────────

[TestFixture]
public class FootballWorldCupStrategyTests
{
    private static List<IContestant> Make32Teams() =>
        Enumerable.Range(0, 32)
            .Select(i => (IContestant)new TeamContestant($"Team{i:D2}"))
            .ToList();

    private static void GiveGroupResults(IEnumerable<ITournament> groups)
    {
        foreach (var g in groups)
            for (int j = 0; j < g.Contestants.Count; j++)
                g.SetResult(g.Contestants[j],
                    new FootballLeaderboardScore(wins: g.Contestants.Count - j, draws: 0, losses: 0));
    }

    [Test]
    public void CreateSubTournaments_Creates8Groups()
    {
        var strategy = new FootballWorldCupStrategy();
        Assert.That(strategy.CreateSubTournaments(Make32Teams()).Count, Is.EqualTo(8));
    }

    [Test]
    public void CreateSubTournaments_GroupNamesAreAToH()
    {
        var strategy = new FootballWorldCupStrategy();
        var names = strategy.CreateSubTournaments(Make32Teams()).Select(g => g.Name).ToHashSet();

        for (char c = 'A'; c <= 'H'; c++)
            Assert.That(names, Contains.Item($"Group {c}"));
    }

    [Test]
    public void CreateSubTournaments_Each4TeamsPerGroup()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());

        Assert.That(groups.Select(g => g.Contestants.Count), Has.All.EqualTo(4));
    }

    [Test]
    public void CreateSubTournaments_TeamsAssignedInOrderToGroups()
    {
        var strategy = new FootballWorldCupStrategy();
        var teams = Make32Teams();
        var groups = strategy.CreateSubTournaments(teams);

        Assert.That(groups[0].Contestants, Is.EqualTo(teams.Take(4).ToList()));
        Assert.That(groups[7].Contestants, Is.EqualTo(teams.Skip(28).Take(4).ToList()));
    }

    [Test]
    public void CreateNextStage_AfterGroupStage_CreatesOneBracketTournament()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());
        GiveGroupResults(groups);

        var next = strategy.CreateNextStage(groups);

        Assert.That(next, Is.Not.Null);
        Assert.That(next!.Count, Is.EqualTo(1));
        Assert.That(next[0].Name, Is.EqualTo("Bracket Stage"));
    }

    [Test]
    public void CreateNextStage_BracketContains16Teams_TopTwoFromEachGroup()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());
        GiveGroupResults(groups);

        var bracket = strategy.CreateNextStage(groups)![0];

        Assert.That(bracket.Contestants.Count, Is.EqualTo(16));
    }

    [Test]
    public void CreateNextStage_TopTeamFromEachGroupAdvances()
    {
        var strategy = new FootballWorldCupStrategy();
        var teams = Make32Teams();
        var groups = strategy.CreateSubTournaments(teams);
        GiveGroupResults(groups); // contestant[0] tops each group

        var bracket = strategy.CreateNextStage(groups)![0];

        // teams[0], [4], [8], ... [28] are group winners
        foreach (var idx in new[] { 0, 4, 8, 12, 16, 20, 24, 28 })
            Assert.That(bracket.Contestants, Contains.Item(teams[idx]), $"teams[{idx}] should advance");
    }

    [Test]
    public void CreateNextStage_UsesFootballTieBreaks_NotPointsOnly()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());

        var groupA = groups[0];
        var a = groupA.Contestants[0];
        var b = groupA.Contestants[1];
        var c = groupA.Contestants[2];
        var d = groupA.Contestants[3];

        // A and B have equal points, but A has better goal difference and should rank above B.
        groupA.SetResult(a, new FootballLeaderboardScore(wins: 2, draws: 0, losses: 1, goalsScored: 5, goalsConceded: 1));
        groupA.SetResult(b, new FootballLeaderboardScore(wins: 2, draws: 0, losses: 1, goalsScored: 3, goalsConceded: 2));
        groupA.SetResult(c, new FootballLeaderboardScore(wins: 1, draws: 0, losses: 2, goalsScored: 2, goalsConceded: 4));
        groupA.SetResult(d, new FootballLeaderboardScore(wins: 0, draws: 0, losses: 3, goalsScored: 1, goalsConceded: 4));

        // Provide deterministic results for remaining groups so bracket can be created.
        for (int i = 1; i < groups.Count; i++)
        {
            var g = groups[i];
            for (int j = 0; j < g.Contestants.Count; j++)
                g.SetResult(g.Contestants[j],
                    new FootballLeaderboardScore(wins: g.Contestants.Count - j, draws: 0, losses: 0));
        }

        var bracket = strategy.CreateNextStage(groups)![0];

        Assert.That(bracket.Contestants, Contains.Item(a));
        Assert.That(bracket.Contestants, Contains.Item(b));
    }

    [Test]
    public void CreateNextStage_SecondCall_ReturnsNull()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());
        GiveGroupResults(groups);

        var withBracket = groups.ToList<ITournament>();
        withBracket.Add(new SingleTournament("Bracket Stage", new FootballBracketStageStrategy()));

        Assert.That(strategy.CreateNextStage(withBracket), Is.Null);
    }

    [Test]
    public void AggregateResults_ReturnsBracketResults_WhenAvailable()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());
        var champion = groups[0].Contestants[0];

        var bracket = new SingleTournament("Bracket Stage", new FootballBracketStageStrategy());
        bracket.SetResult(champion, new FootballMatchScore(goalsScored: 5));

        var allTournaments = groups.ToList<ITournament>();
        allTournaments.Add(bracket);

        var results = strategy.AggregateResults(allTournaments);

        Assert.That(results, Contains.Key(champion));
        Assert.That(results[champion].GetValue(), Is.EqualTo(5));
    }

    [Test]
    public void AggregateResults_FallsBackToGroupResults_WhenNoBracket()
    {
        var strategy = new FootballWorldCupStrategy();
        var groups = strategy.CreateSubTournaments(Make32Teams());
        var topTeam = groups[0].Contestants[0];
        groups[0].SetResult(topTeam, new FootballLeaderboardScore(wins: 3, draws: 0, losses: 0));

        var results = strategy.AggregateResults(groups);

        Assert.That(results, Contains.Key(topTeam));
        Assert.That(results[topTeam].GetValue(), Is.EqualTo(9));
    }
}
