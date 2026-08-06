using InvoicePdf.Api.Invoices;

namespace InvoicePdf.Api.Rendering;

/// <summary>
/// Drains the render queue off the request path.
///
/// Rendering happens here so a burst of invoice requests cannot occupy every
/// thread the API has for serving normal traffic.
/// </summary>
public sealed class PdfRenderWorker(
    PdfRenderQueue queue,
    InvoicePdfRenderer renderer,
    ILogger<PdfRenderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
        {
            queue.Update(new RenderResult(job.Id, RenderState.Running, null, null));

            try
            {
                var pdf = await renderer.RenderAsync(job.Invoice, stoppingToken);
                queue.Update(new RenderResult(job.Id, RenderState.Done, pdf, null));

                logger.LogInformation(
                    "Rendered invoice {InvoiceNumber} for job {JobId} ({Bytes} bytes)",
                    job.Invoice.Number, job.Id, pdf.Length);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutting down. Leave the job queued rather than marking it failed.
                throw;
            }
            catch (Exception ex)
            {
                queue.Update(new RenderResult(job.Id, RenderState.Failed, null, ex.Message));
                logger.LogError(ex, "Failed to render invoice for job {JobId}", job.Id);
            }
        }
    }
}
