namespace DocumentManager.Core.Models;

public sealed record ExpedientGenerationRequest(
    DateTime Date,
    string ServiceOrderFolio,
    string EconomicNumber,
    IReadOnlyCollection<DocumentInput> Documents,
    string DestinationDirectory);

public sealed record ExpedientGenerationResult(
    ServiceRecord Record,
    string FinalPdfPath);

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ValidationResult Success { get; } = new(true, []);
}
