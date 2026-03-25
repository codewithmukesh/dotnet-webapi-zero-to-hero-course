using MediatR;

namespace CqrsMediatr.Api.Features.Products.Notifications;

public record ProductCreatedNotification(Guid ProductId, string ProductName) : INotification;
