using AutoMapper;
using Mapping.Api.Contracts;
using Mapping.Api.Domain;

namespace Mapping.Api.Mapping.AutoMapper;

public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<Category, CategoryResponse>();
        CreateMap<Tag, TagResponse>();
    }
}
