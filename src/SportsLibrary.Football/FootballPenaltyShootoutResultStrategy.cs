using SportsLibrary.Core;

namespace SportsLibrary.Football
{
    /// <summary>Resolves a drawn match by counting scored penalties in the shootout (Minute == null).</summary>
    public class FootballPenaltyShootoutResultStrategy : IMatchResultStrategy
    {
        public IContestant? DetermineWinner(IMatch match)
        {
            var byContestant = match.Timeline
                .GetEventsByPayloadType<FootballPenaltyPayload>()
                .Where(p => p.Minute == null && p.Contestant != null && p.Scored)
                .GroupBy(p => p.Contestant!)
                .OrderByDescending(g => g.Count())
                .ToList();

            if (byContestant.Count == 0) return null;
            if (byContestant.Count == 1 || byContestant[0].Count() > byContestant[1].Count())
                return byContestant[0].Key;
            return null;
        }
    }
}
