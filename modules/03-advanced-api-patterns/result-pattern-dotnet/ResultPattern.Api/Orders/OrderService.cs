using Microsoft.EntityFrameworkCore;
using ResultPattern.Api.Common;

namespace ResultPattern.Api.Orders;

public sealed class OrderService(OrdersDbContext db)
{
    public async Task<Result<Order>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is not null
            ? order
            : OrderErrors.NotFound(id);
    }

    public async Task<Result<Order>> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            return OrderErrors.EmailRequired;
        }

        if (request.Total <= 0)
        {
            return OrderErrors.TotalMustBePositive;
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerEmail = request.CustomerEmail,
            Total = request.Total
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return order;
    }

    /// <summary>
    /// Collects every validation failure instead of stopping at the first one.
    /// Result&lt;T&gt; carries a single Error by design, so the multi-error case
    /// returns the list and the endpoint maps it to a 400 with an errors dictionary.
    /// </summary>
    public IReadOnlyList<Error> Validate(CreateOrderRequest request)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            errors.Add(OrderErrors.EmailRequired);
        }

        if (request.Total <= 0)
        {
            errors.Add(OrderErrors.TotalMustBePositive);
        }

        return errors;
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(id));
        }

        if (order.Status == "Cancelled")
        {
            return Result.Failure(OrderErrors.AlreadyCancelled);
        }

        order.Status = "Cancelled";
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Refund>> RefundAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.Status != "Cancelled")
        {
            return OrderErrors.NotCancelled;
        }

        var refund = new Refund
        {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            Amount = order.Total
        };

        db.Refunds.Add(refund);
        await db.SaveChangesAsync(cancellationToken);

        return refund;
    }

    /// <summary>
    /// Catch #4 in the article, the explicit version. Two lines of plumbing per
    /// real step, verbose but obvious to read six months later.
    /// </summary>
    public async Task<Result<Refund>> CancelAndRefundAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetByIdAsync(orderId, cancellationToken);
        if (order.IsFailure) return order.Error;

        var cancelled = await CancelAsync(orderId, cancellationToken);
        if (cancelled.IsFailure) return cancelled.Error;

        var refreshed = await GetByIdAsync(orderId, cancellationToken);
        if (refreshed.IsFailure) return refreshed.Error;

        var refund = await RefundAsync(refreshed.Value, cancellationToken);
        if (refund.IsFailure) return refund.Error;

        return refund.Value;
    }

    /// <summary>
    /// The same thing through BindAsync. Shorter, and now the codebase owns a
    /// small extension library. Note the middle step: CancelAsync returns the
    /// non-generic Result, which does not fit BindAsync, so it is adapted by hand.
    /// That is the friction catch #4 is about.
    /// </summary>
    public Task<Result<Refund>> CancelAndRefundChainedAsync(Guid orderId, CancellationToken cancellationToken) =>
        GetByIdAsync(orderId, cancellationToken)
            .BindAsync(async order =>
            {
                var cancelled = await CancelAsync(order.Id, cancellationToken);
                return cancelled.IsFailure
                    ? Result<Order>.Failure(cancelled.Error)
                    : await GetByIdAsync(order.Id, cancellationToken);
            })
            .BindAsync(order => RefundAsync(order, cancellationToken));
}
