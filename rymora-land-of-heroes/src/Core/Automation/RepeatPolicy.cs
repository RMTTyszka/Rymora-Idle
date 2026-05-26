namespace RymoraLandOfHeroes.Core.Automation;

public enum RepeatMode
{
    Once,
    Forever,
    Count,
    Duration
}

public sealed record RepeatPolicy
{
    private RepeatPolicy(RepeatMode mode, int? count, float? seconds)
    {
        Mode = mode;
        RepeatCount = count;
        Seconds = seconds;
    }

    public RepeatMode Mode { get; }
    public int? RepeatCount { get; }
    public float? Seconds { get; }

    public static RepeatPolicy Once { get; } = new(RepeatMode.Once, count: 1, seconds: null);
    public static RepeatPolicy Forever { get; } = new(RepeatMode.Forever, count: null, seconds: null);

    public static RepeatPolicy Count(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Repeat count must be positive.");
        }

        return new RepeatPolicy(RepeatMode.Count, count, seconds: null);
    }

    public static RepeatPolicy Duration(float seconds)
    {
        if (!float.IsFinite(seconds) || seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Repeat duration must be positive.");
        }

        return new RepeatPolicy(RepeatMode.Duration, count: null, seconds);
    }
}
