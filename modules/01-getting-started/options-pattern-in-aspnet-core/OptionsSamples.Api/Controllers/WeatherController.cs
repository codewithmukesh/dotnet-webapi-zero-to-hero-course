using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OptionsSamples.Api.Options;

namespace OptionsSamples.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController(
    IOptions<WeatherOptions> options,
    IOptionsSnapshot<WeatherOptions> optionsSnapshot,
    IOptionsMonitor<WeatherOptions> optionsMonitor) : ControllerBase
{
    // IOptions - singleton, reads config once at startup
    [HttpGet("options")]
    public IActionResult GetFromOptions()
    {
        return Ok(options.Value);
    }

    // IOptionsSnapshot - scoped, reads config per request
    [HttpGet("snapshot")]
    public IActionResult GetFromSnapshot()
    {
        return Ok(optionsSnapshot.Value);
    }

    // IOptionsMonitor - singleton, reads current value in real time
    [HttpGet("monitor")]
    public IActionResult GetFromMonitor()
    {
        return Ok(optionsMonitor.CurrentValue);
    }

    // Compare all three interfaces side by side
    [HttpGet("compare")]
    public IActionResult CompareAll()
    {
        return Ok(new
        {
            IOptions = options.Value,
            IOptionsSnapshot = optionsSnapshot.Value,
            IOptionsMonitor = optionsMonitor.CurrentValue
        });
    }
}
