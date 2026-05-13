using Mapping.Api.Contracts;
using Mapping.Api.Domain;
using Mapster;

namespace Mapping.Api.Mapping.Mapster;

public static class ProductMapsterConfig
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductResponse>();
        config.NewConfig<Category, CategoryResponse>();
        config.NewConfig<Tag, TagResponse>();

        config.Compile();
    }
}
