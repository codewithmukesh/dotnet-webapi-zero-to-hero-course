# Validation with MediatR Pipeline Behavior and FluentValidation (.NET 10)

Companion code for the article [Validation with MediatR Pipeline Behavior and FluentValidation](https://codewithmukesh.com/blog/validation-with-mediatr-pipeline-behavior-and-fluentvalidation/).

A minimal CQRS Web API that validates commands inside the MediatR pipeline, so handlers stay free of validation code:

- A `ValidationBehavior` that runs FluentValidation validators before the handler
- A `RequestResponseLoggingBehavior` that logs every request and response
- A `GlobalExceptionHandler` (`IExceptionHandler`) that turns validation failures into a Problem Details response

## Run it

```bash
dotnet run --project ValidationPipeline.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `ValidationPipeline.Api/requests.http`.

## A note on MediatR licensing

MediatR moved to a commercial license (Lucky Penny Software) from version 13.0. It is free for individuals, open-source, non-profits, and companies under $5M annual revenue (register a free key at mediatr.io); larger commercial use needs a paid license. A missing key only logs a startup warning. The pipeline behavior pattern shown here transfers directly to any mediator alternative.

## Stack

.NET 10 · Minimal APIs · MediatR 14.1 · FluentValidation 12 · EF Core 10 (InMemory) · Scalar
