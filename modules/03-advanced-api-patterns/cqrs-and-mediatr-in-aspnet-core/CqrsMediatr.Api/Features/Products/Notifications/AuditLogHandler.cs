using MediatR;

namespace CqrsMediatr.Api.Features.Products.Notifications;

public class AuditLogHandler(ILogger<AuditLogHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling notification for product creation with id: {ProductId}. Writing audit log.",
            notification.ProductId);

        return Task.CompletedTask;
    }
}
