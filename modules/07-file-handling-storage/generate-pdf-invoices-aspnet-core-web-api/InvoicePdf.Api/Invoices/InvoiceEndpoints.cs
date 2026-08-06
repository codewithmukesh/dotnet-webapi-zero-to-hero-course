using InvoicePdf.Api.Rendering;

namespace InvoicePdf.Api.Invoices;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var invoices = app.MapGroup("/invoices").WithTags("Invoices");

        // The direct path. Fine for a handful of invoices a minute, and the
        // shape most tutorials stop at.
        invoices.MapGet("/{number}/pdf", async (
            string number,
            InvoicePdfRenderer renderer,
            CancellationToken cancellationToken) =>
        {
            var invoice = InvoiceStore.GetSample(number);
            var pdf = await renderer.RenderAsync(invoice, cancellationToken);

            return Results.File(pdf, "application/pdf", $"{invoice.Number}.pdf");
        })
        .WithName("GetInvoicePdf")
        .WithSummary("Renders an invoice to PDF on the request thread.")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf");

        // The path that survives volume. Enqueue and answer immediately.
        invoices.MapPost("/{number}/pdf-jobs", (
            string number,
            PdfRenderQueue queue) =>
        {
            var invoice = InvoiceStore.GetSample(number);
            var jobId = queue.TryEnqueue(invoice);

            if (jobId is null)
            {
                return Results.Problem(
                    title: "Render queue is full",
                    detail: "The renderer is saturated. Retry shortly.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            return Results.AcceptedAtRoute("GetRenderJob", new { id = jobId.Value }, new { jobId });
        })
        .WithName("QueueInvoicePdf")
        .WithSummary("Queues an invoice render and returns a job id.");

        app.MapGet("/pdf-jobs/{id:guid}", (Guid id, PdfRenderQueue queue) =>
        {
            var result = queue.Get(id);

            return result switch
            {
                null => Results.NotFound(),
                { State: RenderState.Done, Pdf: { } pdf } =>
                    Results.File(pdf, "application/pdf", $"{id}.pdf"),
                { State: RenderState.Failed } r =>
                    Results.Problem(title: "Render failed", detail: r.Error, statusCode: 500),
                var r => Results.Ok(new { id, state = r.State.ToString() }),
            };
        })
        .WithName("GetRenderJob")
        .WithTags("Invoices")
        .WithSummary("Returns job status, or the PDF once rendering finished.");

        return app;
    }
}
