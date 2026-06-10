using SportsLibrary.Core;

namespace ModelTests;

// ─── Organization hierarchy ───────────────────────────────────────────────────

[TestFixture]
public class OrganizationTests
{
    [Test]
    public void Id_UniquePerInstance()
    {
        var o1 = new Country("Poland", CountryCode.Poland);
        var o2 = new Country("Germany", CountryCode.Germany);
        Assert.That(o1.Id, Is.Not.EqualTo(o2.Id));
    }

    [Test]
    public void Name_SetViaConstructor()
    {
        var org = new Country("Poland", CountryCode.Poland);
        Assert.That(org.Name, Is.EqualTo("Poland"));
    }

    [Test]
    public void Members_EmptyByDefault()
    {
        var org = new Country("Poland", CountryCode.Poland);
        Assert.That(org.Members, Is.Empty);
    }

    [Test]
    public void Members_CanAddAndRetrieveContestants()
    {
        var org = new Country("Poland", CountryCode.Poland);
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
        var country = new Country("Poland", CountryCode.Poland);
        Assert.That(country.Name, Is.EqualTo("Poland"));
    }

    [Test]
    public void Country_Members_EmptyByDefault()
    {
        var country = new Country("Germany", CountryCode.Germany);
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