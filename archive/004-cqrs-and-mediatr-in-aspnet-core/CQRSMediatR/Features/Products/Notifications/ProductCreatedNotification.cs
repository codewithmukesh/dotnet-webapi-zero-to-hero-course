using MediatR;

namespace CQRSMediatR.Features.Products.Notifications;

public record ProductCreatedNotification(Guid Id) : INotification;
