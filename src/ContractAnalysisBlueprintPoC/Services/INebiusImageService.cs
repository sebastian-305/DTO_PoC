using ContractAnalysisBlueprintPoC.Models;

namespace ContractAnalysisBlueprintPoC.Services;

public interface INebiusImageService
{
    Task<ImageGenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
}
