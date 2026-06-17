namespace SportsLibrary.Core
{
    public static class DisqualificationResolver
    {
        public static IReadOnlySet<IContestant> GetDisqualifiedContestants(Match match)
        {
            ArgumentNullException.ThrowIfNull(match);

            return match.GetDisqualifiedContestants().ToHashSet();
        }

        public static bool IsDisqualified(Match match, IContestant contestant)
        {
            ArgumentNullException.ThrowIfNull(match);
            ArgumentNullException.ThrowIfNull(contestant);

            return match.IsDisqualified(contestant);
        }
    }
}