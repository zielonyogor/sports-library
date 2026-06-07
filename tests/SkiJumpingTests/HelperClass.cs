using System.Collections.ObjectModel;
using SportsLibrary.Core;
using SportsLibrary.SkiJumping;

namespace SkiJumpingTests;

// ─── helpers shared by all fixtures ──────────────────────────────────────────

internal static class H
{
    public static IContestant C(string name) =>
        new SingleContestant(name, new Person(name, ""));

    public static List<IContestant> Cs(params string[] names) =>
        names.Select(C).ToList();

    /// <summary>Creates n contestants named A01…A{n:D2}.</summary>
    public static List<IContestant> NumCs(int n) =>
        Enumerable.Range(1, n).Select(i => C($"A{i:D2}")).ToList();

    public static IScore Pts(float v) => new SkiJumpingScore(v, 0f, 0f, 0f);

    public static int Idx(IContestant c) => int.Parse(c.Name[1..]);
}