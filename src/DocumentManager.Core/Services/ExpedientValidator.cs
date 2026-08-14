using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services;

public sealed class ExpedientValidator
{
    public ValidationResult Validate(
        string? serviceOrderFolio,
        string? economicNumber,
        IEnumerable<DocumentInput> documents)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(serviceOrderFolio))
        {
            errors.Add("Escribe el folio de la orden de servicio.");
        }

        if (string.IsNullOrWhiteSpace(economicNumber))
        {
            errors.Add("Escribe el número económico de la unidad.");
        }

        var materialized = documents?.ToArray() ?? [];
        foreach (var type in DocumentOrder.Required)
        {
            var matches = materialized.Where(document => document.Type == type).ToArray();
            if (matches.Length == 0)
            {
                errors.Add($"Falta el documento: {GetDisplayName(type)}.");
            }
            else if (matches.Length > 1)
            {
                errors.Add($"El documento {GetDisplayName(type)} está duplicado.");
            }
            else if (string.IsNullOrWhiteSpace(matches[0].SourcePath) || !File.Exists(matches[0].SourcePath))
            {
                errors.Add($"No se encontró el archivo de {GetDisplayName(type)}.");
            }
        }

        return errors.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(false, errors);
    }

    public static string GetDisplayName(DocumentType type) => type switch
    {
        DocumentType.ServiceOrder => "Orden de servicio",
        DocumentType.WorkOrder => "Orden de trabajo",
        DocumentType.Quote => "Cotización",
        DocumentType.MaintenanceReport => "Reporte de mantenimiento",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
