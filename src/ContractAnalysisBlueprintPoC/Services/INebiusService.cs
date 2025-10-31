using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Services;

public interface INebiusService
{
    Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default);
}
