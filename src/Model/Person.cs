namespace SportsLibrary.Model
{
    public class Person
    {
        Guid Id { set; }
        string Name { get; set; }
        string Surname { get; set; }
        DateOnly BirthDate { get; set; }
    }
}