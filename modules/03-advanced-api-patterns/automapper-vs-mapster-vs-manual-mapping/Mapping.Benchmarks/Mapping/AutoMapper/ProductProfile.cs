using AutoMapper;
using Mapping.Benchmarks.Contracts;
using Mapping.Benchmarks.Domain;

namespace Mapping.Benchmarks.Mapping.AutoMapper;

public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<Category, CategoryResponse>();
        CreateMap<Tag, TagResponse>();
    }
}
