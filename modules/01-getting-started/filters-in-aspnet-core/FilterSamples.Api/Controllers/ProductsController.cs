using FilterSamples.Api.Filters;
using FilterSamples.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FilterSamples.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LoggingActionFilter))]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> Products =
    [
        new(1, "Keyboard", 79.99m, "Mechanical keyboard"),
        new(2, "Mouse", 49.99m, "Wireless mouse"),
        new(3, "Monitor", 399.99m, "27-inch 4K display")
    ];

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Products);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [ServiceFilter(typeof(ValidationActionFilter))]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        var product = new Product(Products.Count + 1, request.Name, request.Price, request.Description);
        Products.Add(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
