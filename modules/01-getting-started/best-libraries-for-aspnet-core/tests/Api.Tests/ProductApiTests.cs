using System.Net;
using System.Net.Http.Json;
using Api.Products;

namespace Api.Tests;

public class ProductApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Created_product_can_be_read_back()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/products",
            new CreateProductRequest("Mechanical Keyboard", "KEY-1001", 89.99m),
            ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        var fetched = await client.GetFromJsonAsync<ProductDto>($"/products/{created!.Id}", ct);

        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Invalid_sku_returns_400()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/products",
            new CreateProductRequest("Mechanical Keyboard", "wrong-sku", 89.99m),
            ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
