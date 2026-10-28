using FluentValidation;
using Hybrid.Api.Shared;
using Hybrid.Domain.Products;
using Hybrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hybrid.Api.Features.Products;

public static class CreateProduct
{
    public sealed record Request(string Name, string Description, decimal Price, int Stock);

    public sealed record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost("/api/products", Handle)
                .AddEndpointFilter<ValidationFilter<Request>>()
                .ProducesValidationProblem()
                .WithName(nameof(CreateProduct))
                .WithTags("Products");
    }

    internal static async Task<Created<Response>> Handle(Request request, AppDbContext db, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.Stock);

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var response = new Response(product.Id, product.Name, product.Description, product.Price, product.Stock, product.CreatedAt);
        return TypedResults.Created($"/api/products/{product.Id}", response);
    }
}
