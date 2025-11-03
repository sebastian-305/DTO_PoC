namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusOptions
{
    public string Model { get; set; } = "";

    public string ImageModel { get; set; } = "";

    public string ApiKey { get; set; } = "";

    public Uri Endpoint { get; set; } = new("https://api.studio.nebius.com/v1/");
}
