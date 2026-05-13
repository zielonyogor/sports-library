using SportsLibrary.Football;
using SportsLibrary.Core;

namespace tests;

// ─── Score types ──────────────────────────────────────────────────────────────

[TestFixture]
public class FootballMatchScoreTests
{
    [Test]
    public void GetValue_ReturnsGoalsScored()
    {
        var score = new FootballMatchScore { GoalsScored = 3 };
        Assert.That(score.GetValue(), Is.EqualTo(3));
    }

    [Test]
    public void GetValue_Zero_WhenNoGoals()
    {
        var score = new FootballMatchScore { GoalsScored = 0 };
        Assert.That(score.GetValue(), Is.EqualTo(0));
    }

    [Test]
    public void Cards_EmptyByDefault()
    {
        var score = new FootballMatchScore();
        Assert.That(score.Cards, Is.Empty);
    }

    [Test]
    public void Cards_CanStoreMultipleCardTypes()
    {
        var score = new FootballMatchScore();
        score.Cards["Player1"] = CardType.Yellow;
        score.Cards["Player2"] = CardType.Red;

        Assert.That(score.Cards["Player1"], Is.EqualTo(CardType.Yellow));
        Assert.That(score.Cards["Player2"], Is.EqualTo(CardType.Red));
    }

    [Test]
    public void Result_CanBeSetToAllOutcomes()
    {
        foreach (var outcome in Enum.GetValues<MatchOutcome>())
        {
            var score = new FootballMatchScore { Result = outcome };
            Assert.That(score.Result, Is.EqualTo(outcome));
        }
    }
}

[TestFixture]
public class FootballLeaderboardScoreTests
{
    [Test]
    public void GetValue_ReturnsPoints()
    {
        var score = new FootballLeaderboardScore { Points = 9 };
        Assert.That(score.GetValue(), Is.EqualTo(9));
    }

    [Test]
    public void GoalDifference_ScoredMinusConceded()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 7, GoalsConceded = 3 };
        Assert.That(score.GoalDifference, Is.EqualTo(4));
    }

    [Test]
    public void GoalDifference_NegativeWhenConcededMore()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 1, GoalsConceded = 4 };
        Assert.That(score.GoalDifference, Is.EqualTo(-3));
    }

    [Test]
    public void GoalDifference_ZeroWhenEqual()
    {
        var score = new FootballLeaderboardScore { GoalsScored = 2, GoalsConceded = 2 };
        Assert.That(score.GoalDifference, Is.EqualTo(0));
    }

    [Test]
    public void WinsDrawsLosses_DefaultZero()
    {
        var score = new FootballLeaderboardScore();
        Assert.That(score.Wins,   Is.EqualTo(0));
        Assert.That(score.Draws,  Is.EqualTo(0));
        Assert.That(score.Losses, Is.EqualTo(0));
    }

    [Test]
    public void Points_StandardLeague_ThreeForWinOneForDraw()
    {
        // Sanity: 3 wins + 2 draws + 1 loss = 11 points
        var score = new FootballLeaderboardScore
        {
            Wins = 3, Draws = 2, Losses = 1,
            Points = 3 * 3 + 2 * 1 + 1 * 0,
        };
        Assert.That(score.GetValue(), Is.EqualTo(11));
    }
}

// ─── FootballGroupStageStrategy ───────────────────────────────────────────────

[TestFixture]
public class FootballGroupStageStrategyTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    [Test]
    public void CreateMatches_4Teams_Creates6RoundRobinMatches()
    {
        var strategy = new FootballGroupStageStrategy();
        var matches = strategy.CreateMatches(new[] { T("A"), T("B"), T("C"), T("D") }.ToList<IContestant>());

        Assert.That(matches.Count, Is.EqualTo(6));
    }

    [Test]
    public void CreateMatches_3Teams_Creates3Matches()
    {
        var strategy = new FootballGroupStageStrategy();
        var matches = strategy.CreateMatches(new[] { T("A"), T("B"), T("C") }.ToList<IContestant>());

        Assert.That(matches.Count, Is.EqualTo(3));
    }

    [Test]
    public void CreateMatches_2Teams_Creates1Match()
    {
        var strategy = new FootballGroupStageStrategy();
        var matches = strategy.CreateMatches(new[] { T("A"), T("B") }.ToList<IContestant>());

        Assert.That(matches.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateMatches_EachPairMeetsExactlyOnce()
    {
        var strategy = new FootballGroupStageStrategy();
        var teams = new[] { T("A"), T("B"), T("C"), T("D") }.ToList<IContestant>();
        var matches = strategy.CreateMatches(teams);

        for (int i = 0; i < teams.Count; i++)
            for (int j = i + 1; j < teams.Count; j++)
            {
                int timesTheyMeet = matches.Count(m =>
                    m.Contestants.Contains(teams[i]) && m.Contestants.Contains(teams[j]));
                Assert.That(timesTheyMeet, Is.EqualTo(1), $"{teams[i].Name} vs {teams[j].Name}");
            }
    }

    [Test]
    public void CreateMatches_AllMatchesHaveExactly2Contestants()
    {
        var strategy = new FootballGroupStageStrategy();
        var matches = strategy.CreateMatches(
            Enumerable.Range(0, 4).Select(i => T($"T{i}")).ToList<IContestant>());

        Assert.That(matches.Select(m => m.Contestants.Count), Has.All.EqualTo(2));
    }

    [Test]
    public void CreateNextRound_AlwaysReturnsNull()
    {
        var strategy = new FootballGroupStageStrategy();
        Assert.That(strategy.CreateNextRound(new List<IMatch>()), Is.Null);
    }
}

// ─── FootballBracketStageStrategy ─────────────────────────────────────────────

[TestFixture]
public class FootballBracketStageStrategyTests
{
    private static IContestant T(string name) => new TeamContestant(name);

    private static List<IContestant> Teams(params string[] names) =>
        names.Select(T).ToList<IContestant>();

    private static void Win(IMatch match, IContestant winner, int winnerGoals = 1, int loserGoals = 0)
    {
        var loser = match.Contestants.First(c => c != winner);
        match.Statistics[winner] = new FootballMatchScore { GoalsScored = winnerGoals, Result = MatchOutcome.Win };
        match.Statistics[loser]  = new FootballMatchScore { GoalsScored = loserGoals,  Result = MatchOutcome.Lose };
    }

    private static void Draw(IMatch match, IContestant penaltyWinner)
    {
        foreach (var c in match.Contestants)
            match.Statistics[c] = new FootballMatchScore { GoalsScored = 1, Result = MatchOutcome.Draw };
        ((Match)match).PenaltyWinner = penaltyWinner;
    }

    [Test]
    public void CreateMatches_8Teams_Creates4Matches()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A","B","C","D","E","F","G","H"));

        Assert.That(matches.Count, Is.EqualTo(4));
    }

    [Test]
    public void CreateMatches_AllMatchesHaveExactly2Contestants()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A","B","C","D"));

        Assert.That(matches.Select(m => m.Contestants.Count), Has.All.EqualTo(2));
    }

    [Test]
    public void CreateMatches_NamesIncludeRound1()
    {
        var strategy = new FootballBracketStageStrategy();
        var matches = strategy.CreateMatches(Teams("A","B","C","D"));

        Assert.That(matches, Has.All.Matches<IMatch>(m => m.Name.StartsWith("Round 1")));
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
        Assert.That(round2[0].Contestants,  Contains.Item(green));
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
        Assert.That(round2[0].Contestants,  Does.Not.Contain(red));
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
        var round1 = strategy.CreateMatches(Teams("A","B","C","D"));
        foreach (var m in round1) Win(m, m.Contestants[0]);

        var round2 = strategy.CreateNextRound(round1);

        Assert.That(round2![0].Name, Does.Contain("Round 2"));
    }

    [Test]
    public void CreateNextRound_FullBracket_ChampionEmergesAfterThreeRounds()
    {
        // 8 teams → round of 8 (4 matches) → round of 4 (2 matches) → final (1 match) → winner
        var strategy = new FootballBracketStageStrategy();
        var teams = Teams("A","B","C","D","E","F","G","H");
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
                g.TournamentResults[g.Contestants[j]] =
                    new FootballLeaderboardScore { Points = (g.Contestants.Count - j) * 3 };
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
        bracket.TournamentResults[champion] = new FootballMatchScore { GoalsScored = 5 };

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
        groups[0].TournamentResults[topTeam] = new FootballLeaderboardScore { Points = 9 };

        var results = strategy.AggregateResults(groups);

        Assert.That(results, Contains.Key(topTeam));
        Assert.That(results[topTeam].GetValue(), Is.EqualTo(9));
    }
}
