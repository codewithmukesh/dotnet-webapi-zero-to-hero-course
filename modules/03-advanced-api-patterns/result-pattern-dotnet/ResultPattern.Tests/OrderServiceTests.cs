using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResultPattern.Api.Common;
using ResultPattern.Api.Orders;
using Xunit;

namespace ResultPattern.Tests;

/// <summary>
/// The point of these tests: a failing Result is asserted like any other return
/// value. No Assert.Throws, no exception type to remember, and the assertion
/// targets the stable Error.Code rather than the human-readable description.
/// </summary>
public sealed class OrderServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrdersDbContext _db;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _db = new OrdersDbContext(
            new DbContextOptionsBuilder<OrdersDbContext>().UseSqlite(_connection).Options);

        _db.Database.EnsureCreated();
        _service = new OrderService(_db);
    }

    [Fact]
    public async Task GetByIdAsync_returns_a_NotFound_error_when_the_order_is_missing()
    {
        var result = await _service.GetByIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Orders.NotFound", result.Error.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task CreateAsync_returns_a_validation_error_when_the_email_is_missing()
    {
        var result = await _service.CreateAsync(
            new CreateOrderRequest(CustomerEmail: "", Total: 25m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Orders.EmailRequired", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_returns_the_order_when_the_request_is_valid()
    {
        var result = await _service.CreateAsync(
            new CreateOrderRequest(CustomerEmail: "dev@codewithmukesh.com", Total: 25m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
    }

    [Fact]
    public async Task CancelAsync_returns_a_conflict_error_when_the_order_is_already_cancelled()
    {
        var created = await _service.CreateAsync(
            new CreateOrderRequest(CustomerEmail: "dev@codewithmukesh.com", Total: 25m),
            TestContext.Current.CancellationToken);

        await _service.CancelAsync(created.Value.Id, TestContext.Current.CancellationToken);
        var second = await _service.CancelAsync(created.Value.Id, TestContext.Current.CancellationToken);

        Assert.True(second.IsFailure);
        Assert.Equal("Orders.AlreadyCancelled", second.Error.Code);
        Assert.Equal(ErrorType.Conflict, second.Error.Type);
    }

    [Fact]
    public void Validate_collects_every_failure_instead_of_stopping_at_the_first()
    {
        var errors = _service.Validate(new CreateOrderRequest(CustomerEmail: "", Total: 0m));

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Code == "Orders.EmailRequired");
        Assert.Contains(errors, error => error.Code == "Orders.TotalMustBePositive");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
