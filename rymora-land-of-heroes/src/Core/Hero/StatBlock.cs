namespace RymoraLandOfHeroes.Core.Hero;

public sealed class StatBlock<TStat>
    where TStat : struct, Enum
{
    private readonly Dictionary<TStat, StatInstance> _stats;

    private StatBlock(Dictionary<TStat, StatInstance> stats)
    {
        _stats = stats;
    }

    public StatInstance this[TStat stat] => _stats[stat];

    public IReadOnlyDictionary<TStat, StatInstance> All => _stats;

    public float TotalPoints => _stats.Values.Sum(stat => stat.Points);

    public static StatBlock<TStat> Create(float initialPoints, float valueDivisor)
    {
        var stats = Enum.GetValues<TStat>()
            .ToDictionary(stat => stat, _ => new StatInstance(initialPoints, valueDivisor));

        return new StatBlock<TStat>(stats);
    }
}
