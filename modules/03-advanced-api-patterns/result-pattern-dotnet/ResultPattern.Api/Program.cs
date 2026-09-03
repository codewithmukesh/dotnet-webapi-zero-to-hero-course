using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ResultPattern.Api.Common;
using ResultPattern.Api.Orders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Orders")));
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureCreatedAsync();
}

// Result handles expected failures. This still catches the unexpected ones.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var orders = app.MapGroup("/orders").WithTags("Orders");

orders.MapGet("/{id:guid}", async Task<Results<Ok<Order>, ProblemHttpResult>> (
    Guid id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.GetByIdAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.Ok(result.Value)
        : result.Error.ToProblem();
});

orders.MapPost("/", async Task<Results<Created<Order>, ProblemHttpResult>> (
    CreateOrderRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CreateAsync(request, cancellationToken);

    return result.Match<Results<Created<Order>, ProblemHttpResult>>(
        onSuccess: order => TypedResults.Created($"/orders/{order.Id}", order),
        onFailure: error => error.ToProblem());
});

// Same create, but reporting every validation failure in one response.
orders.MapPost("/bulk-validated", async Task<Results<Created<Order>, ProblemHttpResult>> (
    CreateOrderRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var errors = service.Validate(request);

    if (errors.Count > 0)
    {
        return errors.ToValidationProblem();
    }

    var result = await service.CreateAsync(request, cancellationToken);

    return result.Match<Results<Created<Order>, ProblemHttpResult>>(
        onSuccess: order => TypedResults.Created($"/orders/{order.Id}", order),
        onFailure: error => error.ToProblem());
});

orders.MapPost("/{id:guid}/cancel", async Task<Results<NoContent, ProblemHttpResult>> (
    Guid id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CancelAsync(id, cancellationToken);

    return result.IsSuccess
        ? TypedResults.NoContent()
        : result.Error.ToProblem();
});

// Catch #4: the same workflow written both ways.
orders.MapPost("/{id:guid}/cancel-and-refund", async Task<Results<Ok<Refund>, ProblemHttpResult>> (
    Guid id,
    bool? chained,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = chained == true
        ? await service.CancelAndRefundChainedAsync(id, cancellationToken)
        : await service.CancelAndRefundAsync(id, cancellationToken);

    return result.Match<Results<Ok<Refund>, ProblemHttpResult>>(
        onSuccess: refund => TypedResults.Ok(refund),
        onFailure: error => error.ToProblem());
});

app.Run();

public partial class Program;
