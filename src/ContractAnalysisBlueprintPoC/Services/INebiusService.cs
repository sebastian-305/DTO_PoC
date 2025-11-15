using System.Text.Json.Nodes;

namespace ContractAnalysisBlueprintPoC.Services;

public interface INebiusService
{
    Task<JsonObject> GetPersonInformationAsync(string person, CancellationToken cancellationToken = default);

    Task<JsonObject> GetCountryInformationAsync(string country, CancellationToken cancellationToken = default);
}
