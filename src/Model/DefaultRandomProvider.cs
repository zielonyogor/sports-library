namespace SportsLibrary.Model
{
    public class DefaultRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public DefaultRandomProvider(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public int Next() => _random.Next();
        public int Next(int maxValue) => _random.Next(maxValue);
        public double NextDouble() => _random.NextDouble();
    }
}
