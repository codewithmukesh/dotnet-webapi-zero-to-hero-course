using System.Net;

namespace HttpResilience.Handlers;

public static class TransientHttp
{
    // 408, 429 and every 5xx are worth another try. Other 4xx client errors are not.
    public static bool IsTransient(HttpResponseMessage response) =>
        response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
        || (int)response.StatusCode >= 500;

    public static bool IsTransient(Exception exception) =>
        exception is HttpRequestException or TimeoutException;
}
