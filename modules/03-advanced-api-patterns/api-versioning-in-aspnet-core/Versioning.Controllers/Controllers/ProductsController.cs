using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Versioning.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1() =>
        Ok(new { version = "1.0", products = new[] { "Keyboard", "Mouse" } });

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() =>
        Ok(new { version = "2.0", data = new[] { "Keyboard", "Mouse" }, count = 2 });
}
