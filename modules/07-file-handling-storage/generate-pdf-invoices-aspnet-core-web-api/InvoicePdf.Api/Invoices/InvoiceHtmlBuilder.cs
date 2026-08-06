using System.Globalization;
using System.Net;
using System.Text;

namespace InvoicePdf.Api.Invoices;

/// <summary>
/// Fills the invoice template. Every money value is formatted against the
/// culture that matches the invoice currency, never the culture the server
/// happens to be running under.
/// </summary>
public sealed class InvoiceHtmlBuilder
{
    private readonly string _templatePath;
    private readonly bool _cacheTemplate;
    private string? _cached;

    /// <summary>
    /// Currency code to the culture whose formatting conventions apply. A real
    /// system would carry this on the customer record.
    /// </summary>
    private static readonly Dictionary<string, string> CurrencyCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GBP"] = "en-GB",
        ["USD"] = "en-US",
        ["EUR"] = "de-DE",
        ["INR"] = "en-IN",
        ["JPY"] = "ja-JP",
    };

    public InvoiceHtmlBuilder(IWebHostEnvironment env)
    {
        _templatePath = Path.Combine(env.ContentRootPath, "Invoices", "Templates", "invoice.html");

        // Cache the template in production. In development, re-read it on every
        // request so template edits show up without restarting the app.
        _cacheTemplate = !env.IsDevelopment();
    }

    private string Template
    {
        get
        {
            if (!_cacheTemplate) return File.ReadAllText(_templatePath);
            return _cached ??= File.ReadAllText(_templatePath);
        }
    }

    public string Build(Invoice invoice)
    {
        var culture = ResolveCulture(invoice.CurrencyCode);

        string Money(decimal value) => value.ToString("C", culture);
        string Date(DateOnly value) => value.ToString("dd MMM yyyy", culture);

        var rows = new StringBuilder();
        foreach (var line in invoice.Lines)
        {
            rows.Append(
                $"""
                     <tr>
                       <td class="sku">{Encode(line.Sku)}</td>
                       <td>{Encode(line.Description)}</td>
                       <td class="num">{line.Quantity.ToString("N0", culture)}</td>
                       <td class="num">{Money(line.UnitPrice)}</td>
                       <td class="num">{line.DiscountRate.ToString("P0", culture)}</td>
                       <td class="num">{Money(line.NetAmount)}</td>
                     </tr>

                 """);
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Number"] = Encode(invoice.Number),
            ["IssuedOn"] = Date(invoice.IssuedOn),
            ["DueOn"] = Date(invoice.DueOn),
            ["PaymentTerms"] = Encode(invoice.PaymentTerms),
            ["Lines"] = rows.ToString(),
            ["Subtotal"] = Money(invoice.Subtotal),
            ["TotalDiscount"] = Money(invoice.TotalDiscount),
            ["Tax"] = Money(invoice.Tax),
            ["Total"] = Money(invoice.Total),
            ["TaxRatePercent"] = invoice.TaxRate.ToString("P0", culture),
        };

        AddParty(values, "Seller", invoice.Seller);
        AddParty(values, "Buyer", invoice.Buyer);

        var html = new StringBuilder(Template);
        foreach (var (key, value) in values)
        {
            html.Replace($"{{{{{key}}}}}", value);
        }

        return html.ToString();
    }

    private static void AddParty(Dictionary<string, string> values, string prefix, Party party)
    {
        values[$"{prefix}Name"] = Encode(party.Name);
        values[$"{prefix}Address1"] = Encode(party.AddressLine1);
        values[$"{prefix}Address2"] = Encode(party.AddressLine2);
        values[$"{prefix}City"] = Encode(party.City);
        values[$"{prefix}PostCode"] = Encode(party.PostCode);
        values[$"{prefix}Country"] = Encode(party.Country);
        values[$"{prefix}TaxId"] = Encode(party.TaxId);
    }

    private static CultureInfo ResolveCulture(string currencyCode) =>
        CurrencyCultures.TryGetValue(currencyCode, out var name)
            ? CultureInfo.GetCultureInfo(name)
            : CultureInfo.InvariantCulture;

    /// <summary>
    /// Product descriptions and customer names are user data. They go into an
    /// HTML document, so they get encoded.
    /// </summary>
    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
