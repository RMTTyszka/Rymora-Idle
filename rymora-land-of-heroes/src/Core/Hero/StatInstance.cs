namespace RymoraLandOfHeroes.Core.Hero;

public sealed class StatInstance
{
    public StatInstance(float points, float valueDivisor)
    {
        if (valueDivisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valueDivisor), "Value divisor must be positive.");
        }

        Points = points;
        ValueDivisor = valueDivisor;
    }

    public float Points { get; private set; }
    public float ValueDivisor { get; }

    public float GetValue() => Points / ValueDivisor;

    public void AddPoints(float amount)
    {
        Points += amount;
    }
}
