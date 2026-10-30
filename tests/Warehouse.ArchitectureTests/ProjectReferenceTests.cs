using System.Xml.Linq;

namespace Warehouse.ArchitectureTests;

public class ProjectReferenceTests
{
    [Fact]
    public void Domain_has_no_project_references()
    {
        AssertReferences("src/Warehouse.Domain/Warehouse.Domain.csproj");
    }

    [Fact]
    public void Shared_has_no_project_references()
    {
        AssertReferences("src/Warehouse.Shared/Warehouse.Shared.csproj");
    }

    [Fact]
    public void Application_references_only_domain()
    {
        AssertReferences("src/Warehouse.Application/Warehouse.Application.csproj", "Warehouse.Domain");
    }

    [Fact]
    public void Infrastructure_references_application_and_domain()
    {
        AssertReferences(
            "src/Warehouse.Infrastructure/Warehouse.Infrastructure.csproj",
            "Warehouse.Application",
            "Warehouse.Domain");
    }

    [Fact]
    public void Api_references_application_infrastructure_and_shared()
    {
        AssertReferences(
            "src/Warehouse.Api/Warehouse.Api.csproj",
            "Warehouse.Application",
            "Warehouse.Infrastructure",
            "Warehouse.Shared");
    }

    private static void AssertReferences(string relativeCsproj, params string[] expected)
    {
        var path = Path.Combine(RepoRoot.Locate(), relativeCsproj);
        var actual = XDocument.Load(path)
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => include is not null)
            .Select(include => Path.GetFileNameWithoutExtension(include!))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal), actual);
    }
}
