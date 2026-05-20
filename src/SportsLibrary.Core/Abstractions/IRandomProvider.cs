namespace SportsLibrary.Core
{
    public interface IRandomProvider
    {
        int Next();
        int Next(int maxValue);
        double NextDouble();
    }
}
