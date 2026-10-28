# IHttpClientFactory in .NET 10 - HttpClient and Resilience Done Right

Source code for the article: https://codewithmukesh.com/blog/httpclient-ihttpclientfactory-resilience-dotnet/

## Projects

| Project | Port | What it does |
|---|---|---|
| `HttpClients.Downstream` | 5100 | A small "external" API. `/products` needs an API key, `/flaky` fails twice then succeeds, `/orders` saves the order and then returns 503 for the first two calls. |
| `HttpClients.Api` | 5000 | Calls the downstream API with typed, named and keyed clients, a `DelegatingHandler`, and `AddStandardResilienceHandler()`. Scalar UI at `/scalar`. |
| `HttpClients.SocketDemo` | - | Sends 500 requests with `new HttpClient()` or `IHttpClientFactory` and counts `TIME_WAIT` sockets. |

## Requirements

- .NET 10 SDK (tested on 10.0.401, runtime 10.0.12)
- Microsoft.Extensions.Http.Resilience 10.10.0

## Run the demos

Start the downstream API first, then the main API, each in its own terminal.

```bash
dotnet run --project src/HttpClients.Downstream
dotnet run --project src/HttpClients.Api
```

**Socket exhaustion demo.** Run one mode at a time, and wait a few minutes between runs so old `TIME_WAIT` sockets expire.

```bash
dotnet run --project src/HttpClients.SocketDemo -- factory
dotnet run --project src/HttpClients.SocketDemo -- new
```

**POST retry demo.** Places one order with the default resilience settings, then one with retries disabled for unsafe methods.

```bash
curl -X POST http://localhost:5000/demo/orders/default
curl -X POST http://localhost:5000/demo/orders/safe
```

**Handler order demo.** Shows which handler runs once per call and which runs once per attempt.

```bash
curl http://localhost:5000/demo/handler-order
```

The API key in `appsettings.json` is a demo value. In a real app, keep keys in user secrets or environment variables.
