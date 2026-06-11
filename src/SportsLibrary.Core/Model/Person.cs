namespace SportsLibrary.Core
{
    /// <summary>
    /// Represents a person involved in sports, such as a player or referee. 
    /// Contains basic personal information such as name, birth date, weight, and height.
    /// </summary>
    public sealed class Person
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; init; }
        public string Surname { get; init; }
        public DateOnly BirthDate { get; init; }
        public float Weight { get; set; }
        public float Height { get; set; }

        public Person(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }
    }
}
