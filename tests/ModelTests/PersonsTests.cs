using SportsLibrary.Football;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace ModelTests;

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
        Assert.That(p.Name, Is.EqualTo("Anna"));
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