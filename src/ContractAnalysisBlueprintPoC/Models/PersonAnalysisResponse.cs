using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Models;

public sealed class PersonAnalysisResponse
{
    public required string Person { get; init; }

    public required JsonObject Data { get; init; }
}
