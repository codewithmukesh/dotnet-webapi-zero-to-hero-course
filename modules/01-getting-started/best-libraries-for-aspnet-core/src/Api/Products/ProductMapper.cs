using Riok.Mapperly.Abstractions;

namespace Api.Products;

[Mapper]
public partial class ProductMapper
{
    [MapperIgnoreSource(nameof(Product.CreatedAt))]
    public partial ProductDto ToDto(Product product);
}
