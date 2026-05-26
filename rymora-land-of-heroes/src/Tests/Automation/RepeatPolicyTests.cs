using RymoraLandOfHeroes.Core.Automation;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class RepeatPolicyTests
{
    [Fact]
    public void Count_requires_positive_count()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => RepeatPolicy.Count(0));

        Assert.Equal("count", error.ParamName);
    }

    [Fact]
    public void Duration_requires_positive_seconds()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => RepeatPolicy.Duration(0));

        Assert.Equal("seconds", error.ParamName);
    }

    [Fact]
    public void Once_has_expected_mode()
    {
        Assert.Equal(RepeatMode.Once, RepeatPolicy.Once.Mode);
    }
}
