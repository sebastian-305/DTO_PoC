using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Models;

public sealed class AnalysisResponse
{
    public required AnalysisType Type { get; init; }

    public required string Query { get; init; }

    public required JsonObject Data { get; init; }
}
