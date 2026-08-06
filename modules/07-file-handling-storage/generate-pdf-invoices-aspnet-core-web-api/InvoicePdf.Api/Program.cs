using InvoicePdf.Api.Invoices;
using InvoicePdf.Api.Rendering;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Matches the "IronPdf": { "LicenseKey": ... } section in appsettings.json and
// the same key in user secrets. Configuration flattens nesting with ':'.
//
// This assignment is required, not belt-and-braces. IronPDF only auto-applies a
// key stored under the flat name "IronPdf.LicenseKey"; a nested section is
// invisible to it, so without these lines every page renders watermarked.
var licenseKey = builder.Configuration["IronPdf:LicenseKey"];
if (!string.IsNullOrWhiteSpace(licenseKey))
{
    IronPdf.License.LicenseKey = licenseKey;
}

builder.Services.AddSingleton<InvoiceHtmlBuilder>();
builder.Services.AddSingleton<InvoicePdfRenderer>();
builder.Services.AddSingleton<PdfRenderQueue>();
builder.Services.AddHostedService<PdfRenderWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapInvoiceEndpoints();

app.Run();
