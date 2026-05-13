using Mapping.Benchmarks.Contracts;
using Mapping.Benchmarks.Domain;
using Riok.Mapperly.Abstractions;

namespace Mapping.Benchmarks.Mapping.Mapperly;

[Mapper]
public partial class ProductMapperlyMapper
{
    public partial ProductResponse ToResponse(Product product);

    public partial CategoryResponse ToResponse(Category category);

    public partial TagResponse ToResponse(Tag tag);
}
