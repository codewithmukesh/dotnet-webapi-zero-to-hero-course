namespace CQRSMediatR.Features.Products.DTOs;
public record ProductDto(Guid Id, string Name, string Description, decimal Price);