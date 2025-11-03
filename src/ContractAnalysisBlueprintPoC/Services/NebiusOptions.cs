namespace ContractAnalysisBlueprintPoC.Services;

public sealed class NebiusOptions
{
    public string Model { get; set; } = "Qwen/Qwen3-30B-A3B";

    public string ImageModel { get; set; } = "stabilityai/stable-diffusion-xl";

    public string ApiKey { get; set; } = "";

    public Uri Endpoint { get; set; } = new("https://api.studio.nebius.com/v1/");
}
