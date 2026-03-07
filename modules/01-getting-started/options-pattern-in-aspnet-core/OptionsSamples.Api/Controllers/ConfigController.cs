using Microsoft.AspNetCore.Mvc;

namespace OptionsSamples.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController(IConfiguration configuration) : ControllerBase
{
    // Bad approach - using IConfiguration directly
    [HttpGet("bad")]
    public IActionResult GetFromConfig()
    {
        var city = configuration.GetValue<string>("WeatherOptions:City");
        var state = configuration.GetValue<string>("WeatherOptions:State");
        var temperature = configuration.GetValue<int>("WeatherOptions:Temperature");
        var summary = configuration.GetValue<string>("WeatherOptions:Summary");
        return Ok(new { City = city, State = state, Temperature = temperature, Summary = summary });
    }
}
