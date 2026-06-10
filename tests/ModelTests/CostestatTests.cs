using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace ModelTests;

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
        team.AddMember(player);
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
        var country = new Country("Poland", CountryCode.Poland);
        team.Organisation = country;
        Assert.That(team.Organisation, Is.SameAs(country));
    }
}