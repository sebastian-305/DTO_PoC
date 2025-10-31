using System.Collections.Generic;
using System.Linq;

namespace ContractAnalysisBlueprintPoC.Blueprints;

public record SummaryField(string Id, string Label, string? Description = null);

public record SectionField(string Id, string Label, SectionFieldKind Kind = SectionFieldKind.Text);

public record SectionBlueprint(string Id, string Title, string Icon, IReadOnlyList<SectionField> Fields);

public record AnalysisBlueprint(
    string Id,
    string DisplayName,
    string SummaryTitle,
    IReadOnlyList<SummaryField> SummaryFields,
    IReadOnlyList<SectionBlueprint> Sections,
    string ConclusionLabel,
    string? CollectiveAgreementLabel = null)
{
    public IEnumerable<string> AllSectionFieldIds() =>
        Sections.SelectMany(section => section.Fields.Select(field => field.Id));
}
