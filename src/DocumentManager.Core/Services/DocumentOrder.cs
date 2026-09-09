using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services;

public static class DocumentOrder
{
    public static readonly IReadOnlyList<DocumentType> Required =
    [
        DocumentType.ServiceOrder,
        DocumentType.WorkOrder,
        DocumentType.Quote,
        DocumentType.MaintenanceReport,
    ];

    public static IReadOnlyList<DocumentInput> Sort(IEnumerable<DocumentInput> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var materialized = documents.ToArray();
        var duplicates = materialized
            .GroupBy(document => document.Type)
            .Where(group => !AllowsMultipleFiles(group.Key) && group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new ArgumentException(
                "El reporte de mantenimiento solo puede contener un archivo.",
                nameof(documents));
        }

        var missing = Required.Where(type => materialized.All(document => document.Type != type)).ToArray();
        if (missing.Length > 0)
        {
            throw new ArgumentException("El expediente debe contener los cuatro tipos de documento requeridos.", nameof(documents));
        }

        return Required
            .SelectMany(type => materialized.Where(document => document.Type == type))
            .ToArray();
    }

    public static bool AllowsMultipleFiles(DocumentType type) => type is
        DocumentType.ServiceOrder or DocumentType.WorkOrder or DocumentType.Quote;

    public static bool AllowsImages(DocumentType type) => type is
        DocumentType.ServiceOrder or DocumentType.WorkOrder;
}
