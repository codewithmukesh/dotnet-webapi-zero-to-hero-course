using Hybrid.Api.Shared;
using Hybrid.Domain.Products;
using Hybrid.Infrastructure.Persistence;
using NetArchTest.Rules;

namespace Hybrid.ArchitectureTests;

public class ArchitectureTests
{
    private const string FeaturesNamespace = "Hybrid.Api.Features";

    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Project_Or_Framework()
    {
        var result = Types.InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Hybrid.Infrastructure",
                "Hybrid.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(typeof(AppDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Hybrid.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Shared_Should_Not_Depend_On_Features()
    {
        var result = Types.InAssembly(typeof(IEndpoint).Assembly)
            .That()
            .ResideInNamespace("Hybrid.Api.Shared")
            .ShouldNot()
            .HaveDependencyOn(FeaturesNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Features_Should_Not_Depend_On_Other_Features()
    {
        var featureNamespaces = typeof(IEndpoint).Assembly.GetTypes()
            .Select(type => type.Namespace)
            .Where(ns => ns is not null && ns.StartsWith($"{FeaturesNamespace}.", StringComparison.Ordinal))
            .Select(ns => ns!)
            .Distinct()
            .ToList();

        foreach (var feature in featureNamespaces)
        {
            var otherFeatures = featureNamespaces.Where(ns => ns != feature).ToArray();
            if (otherFeatures.Length == 0)
            {
                continue;
            }

            var result = Types.InAssembly(typeof(IEndpoint).Assembly)
                .That()
                .ResideInNamespace(feature)
                .ShouldNot()
                .HaveDependencyOnAny(otherFeatures)
                .GetResult();

            Assert.True(result.IsSuccessful, Describe(result));
        }
    }

    private static string Describe(NetArchTest.Rules.TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}";
}
