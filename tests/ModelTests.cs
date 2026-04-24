using SportsLibrary.FootballClasses;
using SportsLibrary.Model;
using SportsLibrary.SkiJumpingClasses;

namespace tests;

// ─── Person ───────────────────────────────────────────────────────────────────

[TestFixture]
public class PersonTests
{
    [Test]
    public void Id_UniquePerInstance()
    {
        var p1 = new Person("Jan", "Kowalski");
        var p2 = new Person("Jan", "Kowalski");
        Assert.That(p1.Id, Is.Not.EqualTo(p2.Id));
    }

    [Test]
    public void NameAndSurname_SetViaConstructor()
    {
        var p = new Person("Anna", "Nowak");
        Assert.That(p.Name,    Is.EqualTo("Anna"));
        Assert.That(p.Surname, Is.EqualTo("Nowak"));
    }

    [Test]
    public void WeightAndHeight_DefaultZero()
    {
        var p = new Person("A", "B");
        Assert.That(p.Weight, Is.EqualTo(0f));
        Assert.That(p.Height, Is.EqualTo(0f));
    }

    [Test]
    public void WeightAndHeight_CanBeSet()
    {
        var p = new Person("A", "B") { Weight = 72.5f, Height = 1.82f };
        Assert.That(p.Weight, Is.EqualTo(72.5f).Within(0.001f));
        Assert.That(p.Height, Is.EqualTo(1.82f).Within(0.001f));
    }

    [Test]
    public void BirthDate_CanBeSet()
    {
        var dob = new DateOnly(1990, 5, 15);
        var p = new Person("A", "B") { BirthDate = dob };
        Assert.That(p.BirthDate, Is.EqualTo(dob));
    }
}

// ─── Organization hierarchy ───────────────────────────────────────────────────

[TestFixture]
public class OrganizationTests
{
    [Test]
    public void Id_UniquePerInstance()
    {
        var o1 = new Organization("Club A");
        var o2 = new Organization("Club B");
        Assert.That(o1.Id, Is.Not.EqualTo(o2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var org = new Organization("FC Warsaw");
        Assert.That(org.Name, Is.EqualTo("FC Warsaw"));
    }

    [Test]
    public void Members_EmptyByDefault()
    {
        var org = new Organization("Club");
        Assert.That(org.Members, Is.Empty);
    }

    [Test]
    public void Members_CanAddAndRetrieveContestants()
    {
        var org = new Organization("Club");
        var member = new SingleContestant("Athlete", new Person("A", "B"));
        org.Members.Add(member);
        Assert.That(org.Members, Contains.Item(member));
    }

    [Test]
    public void SportClub_InheritsNameFromOrganization()
    {
        var club = new SportClub("FC Berlin", CountryCode.Germany);
        Assert.That(club.Name, Is.EqualTo("FC Berlin"));
    }

    [Test]
    public void SportClub_ExposesCountryCode()
    {
        var club = new SportClub("FC Warsaw", CountryCode.Poland);
        Assert.That(club.Country, Is.EqualTo(CountryCode.Poland));
    }

    [Test]
    public void SportClub_Members_EmptyByDefault()
    {
        var club = new SportClub("FC Test", CountryCode.France);
        Assert.That(club.Members, Is.Empty);
    }

    [Test]
    public void Country_InheritsNameFromOrganization()
    {
        var country = new Country("Poland");
        Assert.That(country.Name, Is.EqualTo("Poland"));
    }

    [Test]
    public void Country_Members_EmptyByDefault()
    {
        var country = new Country("Germany");
        Assert.That(country.Members, Is.Empty);
    }

    [Test]
    public void MatchSupervisor_Id_UniquePerInstance()
    {
        var p = new Person("Ref", "Jones");
        var s1 = new MatchSupervisor(p);
        var s2 = new MatchSupervisor(p);
        Assert.That(s1.Id, Is.Not.EqualTo(s2.Id));
    }

    [Test]
    public void MatchSupervisor_ExposesPersonViaConstructor()
    {
        var person = new Person("John", "Smith");
        var supervisor = new MatchSupervisor(person);
        Assert.That(supervisor.Person, Is.SameAs(person));
    }
}

// ─── Contestant types ─────────────────────────────────────────────────────────

[TestFixture]
public class ContestantTests
{
    [Test]
    public void SingleContestant_Id_UniquePerInstance()
    {
        var c1 = new SingleContestant("Athlete", new Person("A", "B"));
        var c2 = new SingleContestant("Athlete", new Person("A", "B"));
        Assert.That(c1.Id, Is.Not.EqualTo(c2.Id));
    }

    [Test]
    public void SingleContestant_Name_SetViaConstructor()
    {
        var c = new SingleContestant("Kamil Stoch", new Person("Kamil", "Stoch"));
        Assert.That(c.Name, Is.EqualTo("Kamil Stoch"));
    }

    [Test]
    public void SingleContestant_Person_SetViaConstructor()
    {
        var person = new Person("Kamil", "Stoch");
        var c = new SingleContestant("Kamil Stoch", person);
        Assert.That(c.Person, Is.SameAs(person));
    }

    [Test]
    public void SingleContestant_Organisation_NullByDefault()
    {
        var c = new SingleContestant("Athlete", new Person("A", "B"));
        Assert.That(c.Organisation, Is.Null);
    }

    [Test]
    public void SingleContestant_Organisation_CanBeAssignedAndRetrieved()
    {
        var c = new SingleContestant("Athlete", new Person("A", "B"));
        var club = new SportClub("FC Test", CountryCode.Poland);
        c.Organisation = club;
        Assert.That(c.Organisation, Is.SameAs(club));
    }

    [Test]
    public void TeamContestant_Id_UniquePerInstance()
    {
        var t1 = new TeamContestant("Team A");
        var t2 = new TeamContestant("Team A");
        Assert.That(t1.Id, Is.Not.EqualTo(t2.Id));
    }

    [Test]
    public void TeamContestant_Members_EmptyByDefault()
    {
        var team = new TeamContestant("Team A");
        Assert.That(team.Members, Is.Empty);
    }

    [Test]
    public void TeamContestant_Members_CanAddPlayers()
    {
        var team = new TeamContestant("Team A");
        var player = new Person("Jan", "Nowak");
        team.Members.Add(player);
        Assert.That(team.Members, Contains.Item(player));
    }

    [Test]
    public void TeamContestant_Organisation_NullByDefault()
    {
        var team = new TeamContestant("Team A");
        Assert.That(team.Organisation, Is.Null);
    }

    [Test]
    public void TeamContestant_Organisation_CanBeAssigned()
    {
        var team = new TeamContestant("Team A");
        var country = new Country("Poland");
        team.Organisation = country;
        Assert.That(team.Organisation, Is.SameAs(country));
    }
}

// ─── Match ────────────────────────────────────────────────────────────────────

[TestFixture]
public class MatchTests
{
    private static IContestant C(string name) => new TeamContestant(name);

    [Test]
    public void Id_UniquePerInstance()
    {
        var m1 = new Match("M", new[] { C("A"), C("B") });
        var m2 = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m1.Id, Is.Not.EqualTo(m2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var m = new Match("Quarter Final", new[] { C("A"), C("B") });
        Assert.That(m.Name, Is.EqualTo("Quarter Final"));
    }

    [Test]
    public void State_ScheduledByDefault()
    {
        var m = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m.State, Is.EqualTo(MatchState.Scheduled));
    }

    [Test]
    public void State_CanBeUpdated()
    {
        var m = new Match("M", new[] { C("A") });
        m.State = MatchState.InProgress;
        Assert.That(m.State, Is.EqualTo(MatchState.InProgress));
    }

    [Test]
    public void Contestants_PopulatedFromConstructor()
    {
        var a = C("A"); var b = C("B");
        var m = new Match("M", new[] { a, b });
        Assert.That(m.Contestants, Is.EquivalentTo(new[] { a, b }));
    }

    [Test]
    public void Statistics_EmptyByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Statistics, Is.Empty);
    }

    [Test]
    public void Statistics_CanStoreScorePerContestant()
    {
        var a = C("A");
        var m = new Match("M", new[] { a });
        var score = new FootballMatchScore { GoalsScored = 2 };
        m.Statistics[a] = score;
        Assert.That(m.Statistics[a], Is.SameAs(score));
    }

    [Test]
    public void PenaltyWinner_NullByDefault()
    {
        var m = new Match("M", new[] { C("A"), C("B") });
        Assert.That(m.PenaltyWinner, Is.Null);
    }

    [Test]
    public void PenaltyWinner_CanBeAssigned()
    {
        var b = C("B");
        var m = new Match("M", new[] { C("A"), b });
        m.PenaltyWinner = b;
        Assert.That(m.PenaltyWinner, Is.SameAs(b));
    }

    [Test]
    public void Timeline_NotNullByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Timeline, Is.Not.Null);
    }

    [Test]
    public void Timeline_EmptyByDefault()
    {
        var m = new Match("M", new[] { C("A") });
        Assert.That(m.Timeline.Events, Is.Empty);
    }
}

// ─── SkiJumpingScore ──────────────────────────────────────────────────────────

[TestFixture]
public class SkiJumpingScoreTests
{
    [Test]
    public void Points_SumsAllFourComponents()
    {
        var score = new SkiJumpingScore(130f, 57f, -2f, 1.2f);
        Assert.That(score.Points, Is.EqualTo(186.2f).Within(0.001f));
    }

    [Test]
    public void GetValue_ReturnsPoints()
    {
        var score = new SkiJumpingScore(130f, 57f, 0f, 0f);
        Assert.That(score.GetValue(), Is.EqualTo(187.0).Within(0.001));
    }

    [Test]
    public void WindCompensation_CanBeNegative_ReducesTotalPoints()
    {
        var withoutWind = new SkiJumpingScore(130f, 57f, 0f, 0f);
        var withHeadwind = new SkiJumpingScore(130f, 57f, -3.6f, 0f);
        Assert.That(withHeadwind.Points, Is.LessThan(withoutWind.Points));
    }

    [Test]
    public void GateCompensation_CanBePositive_IncreasesTotalPoints()
    {
        var withoutGate = new SkiJumpingScore(130f, 57f, 0f, 0f);
        var withGate = new SkiJumpingScore(130f, 57f, 0f, 7.2f);
        Assert.That(withGate.Points, Is.GreaterThan(withoutGate.Points));
    }

    [Test]
    public void AllZeroComponents_PointsAreZero()
    {
        var score = new SkiJumpingScore(0f, 0f, 0f, 0f);
        Assert.That(score.Points, Is.EqualTo(0f));
        Assert.That(score.GetValue(), Is.EqualTo(0.0));
    }

    [Test]
    public void Properties_AreSettableAfterConstruction()
    {
        var score = new SkiJumpingScore(0f, 0f, 0f, 0f);
        score.DistancePoints = 130f;
        score.StylePoints    = 57f;
        Assert.That(score.Points, Is.EqualTo(187f).Within(0.001f));
    }

    [Test]
    public void TwoScores_HigherPointsWins()
    {
        var a = new SkiJumpingScore(135f, 58f, 1f, 0f);   // 194
        var b = new SkiJumpingScore(130f, 57f, -1f, 0f);  // 186
        Assert.That(a.GetValue(), Is.GreaterThan(b.GetValue()));
    }
}
