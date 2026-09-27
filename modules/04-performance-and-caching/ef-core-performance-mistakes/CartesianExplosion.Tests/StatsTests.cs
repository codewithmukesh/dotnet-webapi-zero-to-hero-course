using CartesianExplosion.Measure;

namespace CartesianExplosion.Tests;

public sealed class StatsTests
{
    [Fact]
    public void Median_of_odd_and_even_counts()
    {
        Assert.Equal(3, Stats.Median([5, 1, 3]));
        Assert.Equal(2.5, Stats.Median([4, 1, 3, 2]));
    }

    [Fact]
    public void Percentile_uses_nearest_rank()
    {
        double[] values = [.. Enumerable.Range(1, 100).Select(i => (double)i)];

        Assert.Equal(95, Stats.Percentile(values, 95));
        Assert.Equal(100, Stats.Percentile(values, 100));
    }

    [Fact]
    public void Options_parse_flags_and_values()
    {
        var options = Options.Parse(["--size", "Article", "--documents", "true", "--runs", "7"]);

        Assert.Equal("Article", options.Get("size", "Tiny"));
        Assert.True(options.GetBool("documents", false));
        Assert.Equal(7, options.GetInt("runs", 15));
        Assert.Equal("fallback", options.Get("missing", "fallback"));
    }
}
