using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;


// ─── FootballBracketStageStrategy ─────────────────────────────────────────────

[TestFixture]
public class FootballBracketStageStrategyTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    private static List<IContestant> Teams(params string[] names) =>
        names.Select(T).ToList<IContestant>();

    private static void Win(Match match, IContestant winner, int winnerGoals = 1, int loserGoals = 0)
    {
        var loser = match.Contestants.First(c => c != winner);
        match.SetScore(winner, new FootballMatchScore(winnerGoals, MatchOutcome.Win));
        match.SetScore(loser, new FootballMatchScore(loserGoals, MatchOutcome.Lose));
    }

    private static void Draw(Match match, IContestant penaltyWinner)
    {
        foreach (var c in match.Contestants)
            match.SetScore(c, new FootballMatchScore(1, MatchOutcome.Draw));
        ((Match)match).AssignPenaltyWinner(penaltyWinner);
    }

    [Test]
    public void CreateMatches_8Teams_Creates4Matches()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A", "B", "C", "D", "E", "F", "G", "H"));

        Assert.That(matches.Count, Is.EqualTo(4));
    }

    [Test]
    public void CreateMatches_AllMatchesHaveExactly2Contestants()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A", "B", "C", "D"));

        Assert.That(matches.Select(m => m.Contestants.Count), Has.All.EqualTo(2));
    }

    [Test]
    public void CreateMatches_NamesIncludeRound1()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A", "B", "C", "D"));

        Assert.That(matches, Has.All.Matches<Match>(m => m.Name.StartsWith("Round 1")));
    }

    [Test]
    public void CreateNextRound_WinnerFromEachMatchAdvances()
    {
        var strategy = new FootballBracketStageStrategy();
        var red = T("Red"); var blue = T("Blue"); var green = T("Green"); var yellow = T("Yellow");
        var round1 = strategy.CreateMatches(new List<IContestant> { red, blue, green, yellow });

        Win(round1.First(m => m.Contestants.Contains(red)), red);
        Win(round1.First(m => m.Contestants.Contains(green)), green);

        var round2 = strategy.CreateNextRound(round1);

        Assert.That(round2, Is.Not.Null);
        Assert.That(round2![0].Contestants, Contains.Item(red));
        Assert.That(round2[0].Contestants, Contains.Item(green));
    }

    [Test]
    public void CreateNextRound_DrawResolvedByPenaltyWinner_CorrectTeamAdvances()
    {
        var strategy = new FootballBracketStageStrategy();
        var red = T("Red"); var blue = T("Blue"); var green = T("Green"); var yellow = T("Yellow");
        var round1 = strategy.CreateMatches(new List<IContestant> { red, blue, green, yellow });

        Draw(round1.First(m => m.Contestants.Contains(red)), penaltyWinner: blue);
        Win(round1.First(m => m.Contestants.Contains(green)), green);

        var round2 = strategy.CreateNextRound(round1);

        Assert.That(round2![0].Contestants, Contains.Item(blue));
        Assert.That(round2[0].Contestants, Does.Not.Contain(red));
    }

    [Test]
    public void CreateNextRound_OneContestantRemaining_ReturnsNull()
    {
        var strategy = new FootballBracketStageStrategy();
        var round1 = strategy.CreateMatches(Teams("Red", "Blue"));
        Win(round1[0], T("Red"));

        var round2 = strategy.CreateNextRound(round1);

        Assert.That(round2, Is.Null);
    }

    [Test]
    public void CreateNextRound_RoundNumberIncrementsInMatchName()
    {
        var strategy = new FootballBracketStageStrategy();
        var round1 = strategy.CreateMatches(Teams("A", "B", "C", "D"));
        foreach (var m in round1) Win(m, m.Contestants[0]);

        var round2 = strategy.CreateNextRound(round1);

        Assert.That(round2![0].Name, Does.Contain("Round 2"));
    }

    [Test]
    public void Elimination_DrawResolvedByPenaltyWinner_CorrectContestantAdvances()
    {
        var strategy = new FootballBracketStageStrategy();
        var alpha = T("Alpha"); var beta = T("Beta");
        var gamma = T("Gamma"); var delta = T("Delta");
        var round1 = strategy.CreateMatches(new List<IContestant> { alpha, beta, gamma, delta });

        Draw(round1.First(m => m.Contestants.Contains(alpha)), penaltyWinner: beta);
        Win(round1.First(m => m.Contestants.Contains(gamma)), gamma);

        var round2 = strategy.CreateNextRound(round1)!;
        Assert.That(round2[0].Contestants, Contains.Item(beta));
        Assert.That(round2[0].Contestants, Does.Not.Contain(alpha));
    }

    [Test]
    public void CreateNextRound_FullBracket_ChampionEmergesAfterThreeRounds()
    {
        // 8 teams → round of 8 (4 matches) → round of 4 (2 matches) → final (1 match) → winner
        var strategy = new FootballBracketStageStrategy();
        var teams = Teams("A", "B", "C", "D", "E", "F", "G", "H");
        var teamA = teams[0];

        var r1 = strategy.CreateMatches(teams);
        foreach (var m in r1) Win(m, m.Contestants[0]); // first contestant always wins

        var r2 = strategy.CreateNextRound(r1)!;
        foreach (var m in r2) Win(m, m.Contestants[0]);

        var final = strategy.CreateNextRound(r2)!;
        Win(final[0], teamA);

        var done = strategy.CreateNextRound(final);
        Assert.That(done, Is.Null, "No more rounds after the champion is decided");
    }
}