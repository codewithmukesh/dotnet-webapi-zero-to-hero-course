using Api.Data;
using Dapper;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Api.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("/", async (
            CreateOrder command,
            IMediator mediator,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (command.Quantity <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Quantity"] = ["Quantity must be greater than 0."]
                });
            }

            var productExists = await db.Products.AnyAsync(p => p.Id == command.ProductId, ct);
            if (!productExists)
            {
                return Results.NotFound();
            }

            var created = await mediator.Send(command, ct);
            return Results.Created($"/orders/{created.OrderId}", created);
        });

        group.MapGet("/summary", async (string status, AppDbContext db, CancellationToken ct) =>
        {
            var connection = db.Database.GetDbConnection();

            var rows = await connection.QueryAsync<OrderSummary>(new CommandDefinition(
                """SELECT "Id", "Total", "Status" FROM "Orders" WHERE "Status" = @status""",
                new { status },
                cancellationToken: ct));

            return Results.Ok(rows);
        });

        return app;
    }
}
