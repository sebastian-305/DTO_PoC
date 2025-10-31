namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusOptions
{
    public string Model { get; set; } = "placeholder-model-id";

    public string ApiKey { get; set; } = "placeholder-api-key";

    public Uri Endpoint { get; set; } = new("https://api.studio.nebius.com/v1/");
}
