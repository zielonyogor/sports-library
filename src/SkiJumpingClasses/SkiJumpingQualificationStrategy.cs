using SportsLibrary.Model;

namespace SportsLibrary.SkiJumpingClasses
{
    /// <summary>
    /// All contestants compete in one qualification match; top 30 advance to a finals match.
    /// </summary>
    public class SkiJumpingQualificationStrategy : IMatchesStrategy
    {
        private bool _finalCreated;

        public List<IMatch> CreateMatches(List<IContestant> contestants)
        {
            _finalCreated = false;
            return new List<IMatch> { new Match("Qualification", contestants) };
        }

        public List<IMatch>? CreateNextRound(List<IMatch> completedMatches)
        {
            if (_finalCreated) return null;

            var top30 = completedMatches
                .SelectMany(m => m.Contestants
                    .Select(c => (c, m.Statistics.TryGetValue(c, out var s) ? s.GetValue() : 0d)))
                .OrderByDescending(x => x.Item2)
                .Take(30)
                .Select(x => x.c)
                .ToList();

            _finalCreated = true;
            return new List<IMatch> { new Match("Finals", top30) };
        }
    }
}
