using Testcontainers.PostgreSql;

namespace CartesianExplosion.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    // Not the default "postgres" database: the seeder drops and recreates its database,
    // and Npgsql connects to "postgres" as the admin database to do that.
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("cartesian_explosion")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
