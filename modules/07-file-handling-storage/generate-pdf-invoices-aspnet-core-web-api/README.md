# Generate PDF Invoices from an ASP.NET Core Web API

Companion code for [How to Convert HTML to PDF in C# (.NET 10 Guide)](https://codewithmukesh.com/blog/generate-pdf-invoices-aspnet-core-web-api/).

A Minimal API that renders a B2B wholesale invoice to PDF from an HTML template. The sample order carries 46 line items on purpose, so the document spans two A4 pages and the paging rules are actually exercised rather than described.

## What it demonstrates

- Rendering HTML to PDF with `ChromePdfRenderer` (IronPDF 2026.8.1)
- Print CSS that survives pagination - a real `@page` block, `break-inside: avoid` on rows and the totals block, `tabular-nums` on money columns. The header repeats on page two because `table-header-group` is the browser default, not because the template sets it
- Culture-aware currency formatting driven by the invoice currency, not the server locale
- Page numbers via the renderer's footer, because only the renderer knows the page count
- Returning a PDF from a Minimal API endpoint with `Results.File`
- Moving rendering off the request thread with a bounded `Channel` and a `BackgroundService`

## Requirements

- .NET 10 SDK (built against 10.0.302)
- An IronPDF licence key. There is a free trial at [ironpdf.com](https://ironpdf.com/)

## Setup

The licence key is read from configuration under `IronPdf:LicenseKey` and applied in `Program.cs`. Use user secrets so it never lands in source control:

```bash
cd InvoicePdf.Api
dotnet user-secrets set "IronPdf:LicenseKey" "YOUR-KEY-HERE"
```

`appsettings.json` carries the same key with an empty value if you would rather set it there for a throwaway local run. Do not commit it. In a container, supply `IronPdf__LicenseKey` as an environment variable.

**Do not delete the assignment in `Program.cs`.** IronPDF only auto-applies a key stored under the flat name `IronPdf.LicenseKey`; the nested section this project uses is invisible to it, so those lines are what actually licenses the renderer.

Verify with `IronPdf.License.IsLicensed` before assuming a rendering problem.

Then:

```bash
dotnet run
```

Scalar API reference: `https://localhost:<port>/scalar/v1`

## Endpoints

| Method | Route | What it does |
|---|---|---|
| `GET` | `/invoices/{number}/pdf` | Renders on the request thread and returns the PDF. Simple, and the shape that stops scaling first. |
| `POST` | `/invoices/{number}/pdf-jobs` | Queues a render, returns `202` with a job id. Answers `503` when the queue is saturated. |
| `GET` | `/pdf-jobs/{id}` | Job status, or the PDF once rendering finished. |

Quick check:

```bash
curl -k -o invoice.pdf https://localhost:<port>/invoices/INV-2026-0841/pdf
```

You should get a two-page A4 PDF, roughly 62 KB. Page 2 opens with the repeated table header, both footers read `Page N of 2`, and money renders in GBP (`£15,788.87` subtotal, `£18,946.64` total).

## Running without a licence key

With no key applied, IronPDF falls back to sandbox behaviour:

- It renders **only** in a Development environment **with a debugger attached**. Visual Studio F5 works; `dotnet run`, `dotnet test` and CI all throw `LicensingException: A SANDBOX license key is being used outside of a development environment` - even with `ASPNETCORE_ENVIRONMENT=Development` and a debug build.
- The PDF it does produce is stamped with tiled IRON SOFTWARE watermarks diagonally across every page.

The same applies if the key is present but the config name is wrong, since the lookup just returns null. Check `IronPdf.License.IsLicensed` first - a watermarked render is a licence problem, not a CSS problem.

## Project layout

```
InvoicePdf.Api/
  Invoices/
    Invoice.cs               records + computed totals
    InvoiceStore.cs          46-line sample order
    InvoiceHtmlBuilder.cs    template fill + culture formatting
    InvoicePdfRenderer.cs    ChromePdfRenderer configuration
    InvoiceEndpoints.cs      Minimal API endpoints
    Templates/invoice.html   the actual template
  Rendering/
    PdfRenderQueue.cs        bounded Channel
    PdfRenderWorker.cs       BackgroundService
  Program.cs
InvoicePdf.slnx
```

## Things this sample deliberately does not do

- **Persist rendered PDFs.** Finished bytes sit in a `ConcurrentDictionary` to keep the code readable. Anything real writes them to blob storage and hands back a URL, because an in-memory dictionary of PDFs is an unbounded memory leak.
- **Run in Docker.** Chromium needs fonts and native libraries the `dotnet/aspnet` images do not ship. That is a separate article.
- **Benchmark anything.** No timings are published here or in the article.
