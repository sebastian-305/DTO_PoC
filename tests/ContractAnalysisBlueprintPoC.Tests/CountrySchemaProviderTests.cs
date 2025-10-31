using System.Linq;
using System.Text.Json.Nodes;
using ContractAnalysisBlueprintPoC.Services;
using FluentAssertions;
using Xunit;

namespace ContractAnalysisBlueprintPoC.Tests;

public class CountrySchemaProviderTests
{
    private readonly CountrySchemaProvider _provider = new();

    [Fact]
    public void GetSchema_ShouldReturnClone()
    {
        var first = _provider.GetSchema();
        var second = _provider.GetSchema();

        second["properties"]! ["hauptstadt"]! ["description"] = "verändert";

        first["properties"]! ["hauptstadt"]! ["description"]!
            .GetValue<string>()
            .Should().NotBe("verändert");
    }

    [Fact]
    public void Schema_ShouldContainRequiredFields()
    {
        var schema = _provider.GetSchema();

        schema["type"]!.GetValue<string>().Should().Be("object");
        schema["additionalProperties"]!.GetValue<bool>().Should().BeFalse();

        var required = schema["required"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();
        required.Should().BeEquivalentTo(new[] { "hauptstadt", "einwohner", "flaeche", "sprachen", "kontinent" });

        var properties = schema["properties"]!.AsObject();
        properties.Should().ContainKeys(required);

        properties["sprachen"]!["type"]!.GetValue<string>().Should().Be("array");
        properties["sprachen"]!["items"]!.AsObject()["type"]!.GetValue<string>().Should().Be("string");

        var einwohner = properties["einwohner"]!.AsObject();
        einwohner["type"]!.GetValue<string>().Should().Be("object");
        einwohner["additionalProperties"]!.GetValue<bool>().Should().BeFalse();

        var einwohnerRequired = einwohner["required"]!
            .AsArray()
            .Select(node => node!.GetValue<string>())
            .ToArray();
        einwohnerRequired.Should().BeEquivalentTo(new[] { "anzahl" });

        var einwohnerProperties = einwohner["properties"]!.AsObject();
        einwohnerProperties.Should().ContainKeys("anzahl", "hinweis");
        einwohnerProperties["anzahl"]!["type"]!.GetValue<string>().Should().Be("number");
        einwohnerProperties["hinweis"]!["type"]!.GetValue<string>().Should().Be("string");
    }
}
