using SerilogDemo.Api.Models;

namespace SerilogDemo.Api.Services;

public interface IWeatherService
{
    IEnumerable<WeatherForecast> GetForecast();
}
