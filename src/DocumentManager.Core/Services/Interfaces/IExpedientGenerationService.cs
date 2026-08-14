using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services.Interfaces;

public interface IExpedientGenerationService
{
    Task<ExpedientGenerationResult> GenerateAsync(
        ExpedientGenerationRequest request,
        CancellationToken cancellationToken = default);
}

