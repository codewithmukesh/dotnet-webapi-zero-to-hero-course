using Mapping.Api.Contracts;
using Mapping.Api.Domain;
using Riok.Mapperly.Abstractions;

namespace Mapping.Api.Mapping.Mapperly;

[Mapper]
public partial class ProductMapperlyMapper
{
    public partial ProductResponse ToResponse(Product product);

    public partial CategoryResponse ToResponse(Category category);

    public partial TagResponse ToResponse(Tag tag);
}
