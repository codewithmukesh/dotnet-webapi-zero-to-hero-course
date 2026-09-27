using CartesianExplosion.Api.Data;

namespace CartesianExplosion.Tests;

public sealed class SeedSizeTests
{
    [Fact]
    public void Article_size_matches_the_numbers_in_the_video()
    {
        var size = SeedSize.Article;

        Assert.Equal(30_000, size.SingleQueryRows(withDocuments: false));
        Assert.Equal(300_000, size.SingleQueryRows(withDocuments: true));
        Assert.Equal(2_550, size.RecordCount(withDocuments: false));
        Assert.Equal(3_050, size.RecordCount(withDocuments: true));
    }

    [Fact]
    public void Tiny_size_is_the_three_by_four_example()
    {
        Assert.Equal(12, SeedSize.Tiny.SingleQueryRows(withDocuments: false));
        Assert.Equal(8, SeedSize.Tiny.RecordCount(withDocuments: false));
    }

    [Fact]
    public void An_empty_collection_still_yields_one_row_per_other_child()
    {
        var size = new SeedSize(1, 0, 4, 0);

        Assert.Equal(4, size.SingleQueryRows(withDocuments: false));
    }

    [Theory]
    [InlineData("Tiny", 1, 3, 4, 2)]
    [InlineData("Article", 50, 20, 30, 10)]
    [InlineData("50x40x60x0", 50, 40, 60, 0)]
    public void Parse_reads_named_and_custom_sizes(string value, int d, int p, int e, int doc)
    {
        Assert.Equal(new SeedSize(d, p, e, doc), SeedSize.Parse(value));
    }
}
