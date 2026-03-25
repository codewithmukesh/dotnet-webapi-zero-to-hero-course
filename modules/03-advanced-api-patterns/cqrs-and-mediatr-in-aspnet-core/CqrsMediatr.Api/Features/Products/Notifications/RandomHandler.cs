using MediatR;

namespace CqrsMediatr.Api.Features.Products.Notifications;

public class RandomHandler(ILogger<RandomHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Random handler triggered for product {ProductName} ({ProductId})",
            notification.ProductName, notification.ProductId);

        // This demonstrates that multiple handlers can react to the same notification
        return Task.CompletedTask;
    }
}
