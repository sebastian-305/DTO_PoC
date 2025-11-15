namespace ContractAnalysisBlueprintPoC.Models;

public sealed class AnalysisRequest
{
    public AnalysisType Type { get; set; } = AnalysisType.Person;

    public string? Person { get; set; }

    public string? Country { get; set; }
}
