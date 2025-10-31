using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ContractAnalysisBlueprintPoC.Models;

public class AnalysisResult
{
    [JsonPropertyName("blueprintId")]
    public string BlueprintId { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public Dictionary<string, string> Summary { get; init; } = new();

    [JsonPropertyName("sections")]
    public Dictionary<string, List<Dictionary<string, object?>>> Sections { get; init; } = new();

    [JsonPropertyName("conclusion")]
    public string Conclusion { get; init; } = string.Empty;

    [JsonPropertyName("collectiveAgreement")]
    public string? CollectiveAgreement { get; init; }
}
