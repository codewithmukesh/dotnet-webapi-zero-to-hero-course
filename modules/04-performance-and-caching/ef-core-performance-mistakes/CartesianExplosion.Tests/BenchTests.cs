using CartesianExplosion.Api.Data;
using CartesianExplosion.Measure;

namespace CartesianExplosion.Tests;

public sealed class BenchTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Bench_reports_row_math_for_both_modes()
    {
        var ct = TestContext.Current.CancellationToken;
        var db = new Db(postgres.ConnectionString);
        await using (var context = db.NewContext())
        {
            await Seeder.ResetAsync(context, SeedSize.Tiny, ct);
        }

        var single = await Bench.MeasureAsync(db, "single", documents: false, runs: 3, ct);
        var split = await Bench.MeasureAsync(db, "split", documents: false, runs: 3, ct);

        Assert.Equal(12, single.Rows);
        Assert.Equal(8, split.Rows);
        Assert.Single(single.Statements);
        Assert.Equal(3, split.Statements.Count);
        Assert.True(single.MedianMs > 0);
    }
}
