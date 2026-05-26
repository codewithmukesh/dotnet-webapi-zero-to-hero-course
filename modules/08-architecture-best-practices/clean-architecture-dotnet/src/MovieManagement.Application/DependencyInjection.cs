using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MovieManagement.Application.Movies;

namespace MovieManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMovieService, MovieService>();

        // Register every IValidator in this assembly (CreateMovieRequestValidator, etc.).
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
