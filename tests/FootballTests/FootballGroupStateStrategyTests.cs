using SportsLibrary.Football;
using SportsLibrary.Core;

namespace FootballTests;

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
        Assert.That(strategy.CreateNextRound(new List<Match>()), Is.Null);
    }
}