using SportsLibrary.Models;

namespace SportsLibrary.FootballClasses
{
    public class FootballScore : IScore
    {
        public int goals {get; set;}
        public Dictionary<String, Int> cards {get; set;}
    }
}