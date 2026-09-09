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
            else if (!DocumentOrder.AllowsMultipleFiles(type) && matches.Length > 1)
            {
                errors.Add($"El documento {GetDisplayName(type)} está duplicado.");
            }
            else if (matches.Any(document =>
                         string.IsNullOrWhiteSpace(document.SourcePath) ||
                         !File.Exists(document.SourcePath)))
            {
                errors.Add($"No se encontró uno de los archivos de {GetDisplayName(type)}.");
            }
            else if (matches.Any(document => !IsAllowedFormat(type, document.SourcePath)))
            {
                var formats = DocumentOrder.AllowsImages(type) ? "PDF, PNG, JPG o JPEG" : "PDF";
                errors.Add($"Los archivos de {GetDisplayName(type)} deben estar en formato {formats}.");
            }
        }

        return errors.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(false, errors);
    }

    private static bool IsAllowedFormat(DocumentType type, string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) ||
               DocumentOrder.AllowsImages(type) && extension is not null &&
               (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase));
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
