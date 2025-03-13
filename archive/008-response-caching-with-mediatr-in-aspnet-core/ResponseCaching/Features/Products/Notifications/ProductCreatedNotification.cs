using MediatR;

namespace ResponseCaching.Features.Products.Notifications;

public record ProductCreatedNotification(Guid Id) : INotification;
