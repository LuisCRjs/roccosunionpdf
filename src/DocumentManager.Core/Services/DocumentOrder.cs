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
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new ArgumentException("Cada tipo de documento debe aparecer una sola vez.", nameof(documents));
        }

        var byType = materialized.ToDictionary(document => document.Type);
        var missing = Required.Where(type => !byType.ContainsKey(type)).ToArray();
        if (missing.Length > 0)
        {
            throw new ArgumentException("El expediente debe contener exactamente los cuatro documentos requeridos.", nameof(documents));
        }

        return Required.Select(type => byType[type]).ToArray();
    }
}

