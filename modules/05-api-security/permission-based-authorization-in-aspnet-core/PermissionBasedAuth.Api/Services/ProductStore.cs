using System.Collections.Concurrent;
using PermissionBasedAuth.Api.Models;

namespace PermissionBasedAuth.Api.Services;

// In-memory product list so the sample runs with zero database setup.
public class ProductStore
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _nextId;

    public ProductStore()
    {
        Add(new SaveProductRequest("Mechanical Keyboard", 149.00m));
        Add(new SaveProductRequest("Ultrawide Monitor", 699.00m));
    }

    public IEnumerable<Product> GetAll() => _products.Values.OrderBy(p => p.Id);

    public Product? Get(int id) => _products.TryGetValue(id, out var product) ? product : null;

    public Product Add(SaveProductRequest request)
    {
        var id = Interlocked.Increment(ref _nextId);
        var product = new Product(id, request.Name, request.Price);
        _products[id] = product;
        return product;
    }

    public Product? Update(int id, SaveProductRequest request)
    {
        if (!_products.ContainsKey(id))
        {
            return null;
        }

        var updated = new Product(id, request.Name, request.Price);
        _products[id] = updated;
        return updated;
    }

    public bool Delete(int id) => _products.TryRemove(id, out _);
}
