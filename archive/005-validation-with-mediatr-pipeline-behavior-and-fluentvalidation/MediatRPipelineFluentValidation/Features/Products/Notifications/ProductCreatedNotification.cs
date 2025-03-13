using MediatR;

namespace MediatRPipelineFluentValidation.Features.Products.Notifications;

public record ProductCreatedNotification(Guid Id) : INotification;
