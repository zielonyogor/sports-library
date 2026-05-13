namespace SportsLibrary.Core
{
    public class Person
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateOnly BirthDate { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }

        public Person(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }
    }
}
