using FilterSamples.Api.Filters;

namespace FilterSamples.Api.Extensions;

public static class FilterExtensions
{
    public static IServiceCollection AddFilterServices(this IServiceCollection services)
    {
        services.AddScoped<LoggingActionFilter>();
        services.AddScoped<ValidationActionFilter>();
        services.AddScoped<TimingResourceFilter>();
        services.AddScoped<GlobalExceptionFilter>();
        services.AddScoped<ResponseWrappingResultFilter>();
        return services;
    }
}
