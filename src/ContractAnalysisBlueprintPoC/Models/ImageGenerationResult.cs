namespace ContractAnalysisBlueprintPoC.Models;

public sealed class ImageGenerationResult
{
    public required string Prompt { get; init; }

    public string? ImageUrl { get; init; }

    public string? ImageBase64 { get; init; }

    public string? MediaType { get; init; }
}
