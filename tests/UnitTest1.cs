namespace tests;

public class SportsSimulationTests
{
    [Test]
    public void SkiJumping_UsesAggregateRoundScoreForWinner()
    {
        var contestants = new[]
        {
            new Contestant("Kamil"),
            new Contestant("Dawid"),
            new Contestant("Stefan")
        };

        var firstRound = new Dictionary<Contestant, SkiJumpingScore>
        {
            [contestants[0]] = new(126.5f, 56.0f, -2.1f, 1.0f),
            [contestants[1]] = new(125.0f, 55.5f, 0.4f, 0.3f),
            [contestants[2]] = new(124.0f, 55.0f, 0.2f, 0.0f)
        };

        var secondRound = new Dictionary<Contestant, SkiJumpingScore>
        {
            [contestants[0]] = new(125.0f, 55.0f, -2.0f, 0.7f),
            [contestants[1]] = new(127.5f, 56.0f, 1.2f, 0.5f),
            [contestants[2]] = new(123.5f, 54.5f, 0.1f, 0.0f)
        };

        var winner = SkiJumpingSimulation.SelectWinner(new[] { firstRound, secondRound });

        Assert.That(winner.Name, Is.EqualTo("Dawid"));
    }

    [Test]
    public void Football_GroupRanking_AwardsThreeForWinAndOneForDraw()
    {
        var red = new Contestant("Red");
        var blue = new Contestant("Blue");
        var green = new Contestant("Green");

        var matches = new[]
        {
            new FootballMatchResult(red, blue, new FootballScore(2, 0)),
            new FootballMatchResult(red, green, new FootballScore(1, 1)),
            new FootballMatchResult(blue, green, new FootballScore(0, 0))
        };

        var table = FootballSimulation.BuildGroupTable(matches);

        Assert.That(table[red], Is.EqualTo(4));
        Assert.That(table[green], Is.EqualTo(2));
        Assert.That(table[blue], Is.EqualTo(1));
    }

    [Test]
    public void Football_Elimination_ResolvesDrawWithPenaltyWinner()
    {
        var alpha = new Contestant("Alpha");
        var beta = new Contestant("Beta");

        var regularTime = new FootballScore(2, 2);
        var winner = FootballSimulation.ResolveEliminationWinner(alpha, beta, regularTime, penaltyWinner: beta);

        Assert.That(winner.Name, Is.EqualTo("Beta"));
    }

    private sealed record Contestant(string Name);

    private sealed record SkiJumpingScore(float DistancePoints, float StylePoints, float WindCompensation, float GateCompensation)
    {
        public float Total => DistancePoints + StylePoints + WindCompensation + GateCompensation;
    }

    private sealed record FootballScore(int HomeGoals, int AwayGoals);

    private sealed record FootballMatchResult(Contestant Home, Contestant Away, FootballScore Score);

    private static class SkiJumpingSimulation
    {
        public static Contestant SelectWinner(IEnumerable<Dictionary<Contestant, SkiJumpingScore>> rounds)
        {
            var totals = new Dictionary<Contestant, float>();

            foreach (var round in rounds)
            {
                foreach (var entry in round)
                {
                    totals.TryGetValue(entry.Key, out var sum);
                    totals[entry.Key] = sum + entry.Value.Total;
                }
            }

            return totals
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key.Name, StringComparer.Ordinal)
                .First()
                .Key;
        }
    }

    private static class FootballSimulation
    {
        public static Dictionary<Contestant, int> BuildGroupTable(IEnumerable<FootballMatchResult> matches)
        {
            var points = new Dictionary<Contestant, int>();

            foreach (var match in matches)
            {
                EnsureContestant(points, match.Home);
                EnsureContestant(points, match.Away);

                if (match.Score.HomeGoals > match.Score.AwayGoals)
                {
                    points[match.Home] += 3;
                    continue;
                }

                if (match.Score.HomeGoals < match.Score.AwayGoals)
                {
                    points[match.Away] += 3;
                    continue;
                }

                points[match.Home] += 1;
                points[match.Away] += 1;
            }

            return points;
        }

        public static Contestant ResolveEliminationWinner(
            Contestant home,
            Contestant away,
            FootballScore regularTime,
            Contestant penaltyWinner)
        {
            if (regularTime.HomeGoals > regularTime.AwayGoals)
            {
                return home;
            }

            if (regularTime.HomeGoals < regularTime.AwayGoals)
            {
                return away;
            }

            return penaltyWinner;
        }

        private static void EnsureContestant(Dictionary<Contestant, int> points, Contestant contestant)
        {
            if (!points.ContainsKey(contestant))
            {
                points[contestant] = 0;
            }
        }
    }
}
