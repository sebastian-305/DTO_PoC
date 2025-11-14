using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Models;

public sealed class CountryAnalysisResponse
{
    public required string Country { get; init; }

    public required JsonObject Data { get; init; }

    public ImageGenerationResult? Image { get; init; }

    public string? ImageError { get; init; }
}
