using NetArchTest.Rules;
using TodoCharacters.Application;
using TodoCharacters.Domain;
using TodoCharacters.Infrastructure;
using TodoCharacters.WebApi;

namespace TodoCharacters.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_should_not_reference_Infrastructure_or_WebApi()
    {
        var result = Types.InAssembly(typeof(TodoCharacters.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("TodoCharacters.Infrastructure", "TodoCharacters.WebApi")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_should_not_reference_Infrastructure_or_WebApi()
    {
        var result = Types.InAssembly(typeof(TodoCharacters.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("TodoCharacters.Infrastructure", "TodoCharacters.WebApi")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
