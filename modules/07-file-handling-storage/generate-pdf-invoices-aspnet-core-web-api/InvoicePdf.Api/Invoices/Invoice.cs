namespace InvoicePdf.Api.Invoices;

/// <summary>
/// A party on the invoice - the issuing distributor or the retailer being billed.
/// </summary>
public sealed record Party(
    string Name,
    string AddressLine1,
    string AddressLine2,
    string City,
    string PostCode,
    string Country,
    string TaxId);

/// <summary>
/// A single billed line. Discount is a fraction (0.05 = 5%) applied to the line
/// subtotal before tax.
/// </summary>
public sealed record InvoiceLine(
    string Sku,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate)
{
    public decimal GrossAmount => Quantity * UnitPrice;

    public decimal DiscountAmount => decimal.Round(GrossAmount * DiscountRate, 2, MidpointRounding.AwayFromZero);

    public decimal NetAmount => GrossAmount - DiscountAmount;
}

/// <summary>
/// A B2B wholesale invoice. Totals are computed from the lines rather than
/// stored, so the document can never disagree with itself.
/// </summary>
public sealed record Invoice(
    string Number,
    DateOnly IssuedOn,
    DateOnly DueOn,
    string CurrencyCode,
    decimal TaxRate,
    Party Seller,
    Party Buyer,
    IReadOnlyList<InvoiceLine> Lines,
    string PaymentTerms)
{
    public decimal Subtotal => Lines.Sum(l => l.NetAmount);

    public decimal TotalDiscount => Lines.Sum(l => l.DiscountAmount);

    public decimal Tax => decimal.Round(Subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);

    public decimal Total => Subtotal + Tax;
}
