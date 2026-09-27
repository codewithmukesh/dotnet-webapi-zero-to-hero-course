using CartesianExplosion.Api.Data;
using CartesianExplosion.Api.Queries;
using CartesianExplosion.Measure;
using Microsoft.EntityFrameworkCore;

namespace CartesianExplosion.Tests;

public sealed class CartesianExplosionTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<Db> SeedAsync(SeedSize size, bool isolatedQueryCache = false)
    {
        var db = new Db(postgres.ConnectionString, isolatedQueryCache);
        await using var context = db.NewContext();
        await Seeder.ResetAsync(context, size, Ct);
        db.Capture.Clear();
        return db;
    }

    private static async Task<List<ResultSize.Statement>> RunAndMeasureAsync(Db db, bool split, bool documents)
    {
        db.Capture.Clear();
        await using (var context = db.NewContext())
        {
            var query = split ? DepartmentQueries.Split(context, documents) : DepartmentQueries.Single(context, documents);
            await query.ToListAsync(Ct);
        }

        var statements = new List<ResultSize.Statement>();
        foreach (var sql in db.Capture.Commands)
        {
            statements.Add(await ResultSize.MeasureAsync(db.ConnectionString, sql, Ct));
        }

        return statements;
    }

    [Fact]
    public async Task Single_query_returns_projects_times_employees_rows()
    {
        var db = await SeedAsync(SeedSize.Tiny);

        var statements = await RunAndMeasureAsync(db, split: false, documents: false);

        var statement = Assert.Single(statements);
        Assert.Equal(12, statement.Rows);
        Assert.True(statement.Bytes > 0);
        Assert.Contains("LEFT JOIN", statement.Sql);
    }

    [Fact]
    public async Task Single_query_with_documents_multiplies_again()
    {
        var db = await SeedAsync(SeedSize.Tiny);

        var statements = await RunAndMeasureAsync(db, split: false, documents: true);

        Assert.Equal(24, Assert.Single(statements).Rows);
    }

    [Fact]
    public async Task Split_query_sends_the_parent_plus_one_statement_per_collection()
    {
        var db = await SeedAsync(SeedSize.Tiny);

        var statements = await RunAndMeasureAsync(db, split: true, documents: false);

        Assert.Equal(3, statements.Count);
        Assert.Equal(8, statements.Sum(s => s.Rows));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Both_modes_load_the_same_department(bool split)
    {
        var db = await SeedAsync(SeedSize.Tiny);

        await using var context = db.NewContext();
        var query = split ? DepartmentQueries.Split(context, false) : DepartmentQueries.Single(context, false);
        var engineering = Assert.Single(await query.ToListAsync(Ct));

        Assert.Equal("Engineering", engineering.Name);
        Assert.Equal(new[] { "Aryabhata", "Bhaskara", "Chandrayaan" }, engineering.Projects.Select(p => p.Name).Order());
        Assert.Equal(new[] { "Aarav", "Bhavya", "Chetan", "Divya" }, engineering.Employees.Select(e => e.FullName).Order());
    }

    [Fact]
    public async Task EF_warns_about_multiple_collections_for_single_but_not_split()
    {
        // The warning is logged when a query is compiled, and other tests may already have compiled it.
        var db = await SeedAsync(SeedSize.Tiny, isolatedQueryCache: true);

        await RunAndMeasureAsync(db, split: true, documents: false);
        Assert.Empty(db.Warnings);

        await RunAndMeasureAsync(db, split: false, documents: false);
        Assert.Contains(db.Warnings, w => w.Contains("QuerySplittingBehavior"));
    }

    [Fact]
    public async Task Department_with_no_projects_still_returns_one_row_per_employee()
    {
        var db = await SeedAsync(new SeedSize(1, 0, 4, 0));

        var statements = await RunAndMeasureAsync(db, split: false, documents: false);

        Assert.Equal(4, Assert.Single(statements).Rows);
    }
}
