using CartesianExplosion.Measure;

namespace CartesianExplosion.Tests;

public sealed class LoadTests
{
    [Theory]
    [InlineData(200, null, "ok")]
    [InlineData(503, null, "http-503")]
    [InlineData(500, null, "http-500")]
    [InlineData(0, "TaskCanceledException", "client-timeout")]
    [InlineData(0, "HttpRequestException", "failed")]
    public void Classify_separates_client_timeouts_from_server_errors(int status, string? error, string expected)
    {
        Assert.Equal(expected, Load.Classify(new Load.Sample(10, status, error)));
    }
}
