using MediatR;

namespace CqrsMediatr.Api.Features.Products.Notifications;

public class StockAssignedHandler(ILogger<StockAssignedHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning initial stock for product {ProductName} ({ProductId})",
            notification.ProductName, notification.ProductId);

        // In a real app, this would assign default stock levels
        return Task.CompletedTask;
    }
}
