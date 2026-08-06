using IronPdf;
using IronPdf.Rendering;

namespace InvoicePdf.Api.Invoices;

/// <summary>
/// Renders invoice HTML to PDF bytes.
///
/// The renderer is registered as a singleton. Constructing a
/// <see cref="ChromePdfRenderer"/> is cheap, but the Chromium instance behind
/// it is not, so there is no reason to build a new one per request.
/// </summary>
public sealed class InvoicePdfRenderer(InvoiceHtmlBuilder htmlBuilder)
{
    private readonly ChromePdfRenderer _renderer = CreateRenderer();

    /// <summary>
    /// RenderingOptions is shared mutable state on the singleton renderer, and
    /// the footer carries the invoice number, so two concurrent renders would
    /// stamp each other's documents. One render at a time per renderer.
    ///
    /// This is not a workaround - a render is a full Chromium layout pass, so
    /// running them concurrently on one instance buys nothing anyway. It is
    /// also the reason the queue in Rendering/ exists.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<byte[]> RenderAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var html = htmlBuilder.Build(invoice);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            // Page numbers are resolved by IronPDF after pagination, which is why
            // the footer is configured here rather than baked into the template.
            _renderer.RenderingOptions.TextFooter = new TextHeaderFooter
            {
                LeftText = $"Invoice {invoice.Number}",
                RightText = "Page {page} of {total-pages}",
                DrawDividerLine = true,
                FontSize = 8,
            };

            var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
            return pdf.BinaryData;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static ChromePdfRenderer CreateRenderer()
    {
        var renderer = new ChromePdfRenderer();
        var options = renderer.RenderingOptions;

        options.CssMediaType = PdfCssMediaType.Print;
        options.PaperSize = PdfPaperSize.A4;
        options.PaperOrientation = PdfPaperOrientation.Portrait;

        // Without this, Chromium strips every background colour and the table
        // header renders as black text on white.
        options.PrintHtmlBackgrounds = true;

        // The template already declares its own @page margins. By default the
        // values on RenderingOptions win, which silently overrides the CSS.
        options.CssPageRulePolicy = CssPageRulePolicy.CssPageWin;

        // Reserve space for the footer so it never sits on top of a line item.
        options.MarginBottom = 16;
        options.UseMarginsOnHeaderAndFooter = UseMargins.All;

        // The invoice is static markup. No scripts to run, no reason to pay
        // for a render delay.
        options.EnableJavaScript = false;

        return renderer;
    }
}
