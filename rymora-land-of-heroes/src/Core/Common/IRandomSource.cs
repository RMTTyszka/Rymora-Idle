namespace RymoraLandOfHeroes.Core.Common;

public interface IRandomSource
{
    int NextInclusive(int min, int max);
}

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random;

    public SystemRandomSource(int? seed = null)
    {
        _random = seed is null ? new Random() : new Random(seed.Value);
    }

    public int NextInclusive(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Random min cannot be greater than max.");
        }

        return _random.Next(min, max + 1);
    }
}
