using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mapping.Benchmarks.Contracts;
using Mapping.Benchmarks.Domain;
using Mapping.Benchmarks.Mapping.AutoMapper;
using Mapping.Benchmarks.Mapping.Manual;
using Mapping.Benchmarks.Mapping.Mapperly;
using Mapster;

BenchmarkRunner.Run<MappingBenchmark>();

[MemoryDiagnoser]
public class MappingBenchmark
{
    private readonly Product _product = ProductFactory.Sample();
    private readonly IMapper _autoMapper;
    private readonly ProductMapperlyMapper _mapperly = new();
    private readonly TypeAdapterConfig _mapsterConfig = new();

    public MappingBenchmark()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>());
        _autoMapper = config.CreateMapper();

        _mapsterConfig.NewConfig<Product, ProductResponse>();
        _mapsterConfig.NewConfig<Category, CategoryResponse>();
        _mapsterConfig.NewConfig<Tag, TagResponse>();
        _mapsterConfig.Compile();
    }

    [Benchmark(Baseline = true)]
    public ProductResponse Manual() => _product.ToResponse();

    [Benchmark]
    public ProductResponse Mapperly() => _mapperly.ToResponse(_product);

    [Benchmark]
    public ProductResponse Mapster() => _product.Adapt<ProductResponse>(_mapsterConfig);

    [Benchmark]
    public ProductResponse AutoMapper() => _autoMapper.Map<ProductResponse>(_product);
}
