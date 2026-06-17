using System.Collections.Concurrent;
using RoleBasedAuth.Api.Models;

namespace RoleBasedAuth.Api.Services;

// A tiny in-memory product store. The focus of this sample is authorization,
// not data access, so this stands in for a real database.
public class ProductStore
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _nextId;

    public ProductStore()
    {
        Add(new CreateProductRequest("Mechanical Keyboard", 24));
        Add(new CreateProductRequest("4K Monitor", 11));
        Add(new CreateProductRequest("USB-C Dock", 42));
    }

    public IReadOnlyCollection<Product> GetAll() => _products.Values.OrderBy(p => p.Id).ToList();

    public Product Add(CreateProductRequest request)
    {
        var id = Interlocked.Increment(ref _nextId);
        var product = new Product(id, request.Name, request.Stock);
        _products[id] = product;
        return product;
    }

    public bool Delete(int id) => _products.TryRemove(id, out _);
}
