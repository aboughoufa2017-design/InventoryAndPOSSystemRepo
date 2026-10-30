using NetArchTest.Rules;

namespace Warehouse.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly string[] OuterLayers =
    [
        "Warehouse.Application",
        "Warehouse.Infrastructure",
        "Warehouse.Api",
        "Warehouse.Shared",
    ];

    [Fact]
    public void Domain_types_do_not_depend_on_outer_layers()
    {
        AssertNoDependency(Warehouse.Domain.AssemblyReference.Assembly, OuterLayers);
    }

    [Fact]
    public void Application_types_do_not_depend_on_infrastructure_or_host()
    {
        AssertNoDependency(
            Warehouse.Application.AssemblyReference.Assembly,
            "Warehouse.Infrastructure",
            "Warehouse.Api",
            "Warehouse.Shared");
    }

    [Fact]
    public void Infrastructure_types_do_not_depend_on_the_host()
    {
        AssertNoDependency(Warehouse.Infrastructure.AssemblyReference.Assembly, "Warehouse.Api");
    }

    [Fact]
    public void Shared_types_do_not_depend_on_application_layers()
    {
        AssertNoDependency(
            Warehouse.Shared.AssemblyReference.Assembly,
            "Warehouse.Domain",
            "Warehouse.Application",
            "Warehouse.Infrastructure",
            "Warehouse.Api");
    }

    private static void AssertNoDependency(System.Reflection.Assembly assembly, params string[] forbidden)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            result.FailingTypeNames is null ? "Dependency rule failed." : string.Join(", ", result.FailingTypeNames));
    }
}
