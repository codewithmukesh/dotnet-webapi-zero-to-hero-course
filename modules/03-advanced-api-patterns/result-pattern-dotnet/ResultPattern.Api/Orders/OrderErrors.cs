using ResultPattern.Api.Common;

namespace ResultPattern.Api.Orders;

/// <summary>
/// Every failure this feature can produce, in one place. Grep-able, testable,
/// and impossible to typo into a different error message at each call site.
/// </summary>
public static class OrderErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Orders.NotFound", $"No order was found with the id {id}.");

    public static readonly Error EmailRequired =
        Error.Validation("Orders.EmailRequired", "A customer email is required.");

    public static readonly Error TotalMustBePositive =
        Error.Validation("Orders.TotalMustBePositive", "The order total must be greater than zero.");

    public static readonly Error AlreadyCancelled =
        Error.Conflict("Orders.AlreadyCancelled", "The order has already been cancelled.");

    public static readonly Error NotCancelled =
        Error.Validation("Orders.NotCancelled", "Only a cancelled order can be refunded.");
}
