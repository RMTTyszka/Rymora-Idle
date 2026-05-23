using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Tests;

internal sealed class SequenceRandomSource : IRandomSource
{
    private readonly Queue<int> _values;

    public SequenceRandomSource(params int[] values)
    {
        _values = new Queue<int>(values);
    }

    public int NextInclusive(int min, int max)
    {
        if (_values.Count == 0)
        {
            throw new InvalidOperationException("Sequence random source has no values.");
        }

        var value = _values.Dequeue();
        if (value < min || value > max)
        {
            throw new InvalidOperationException($"Random value {value} outside [{min}, {max}].");
        }

        return value;
    }
}
