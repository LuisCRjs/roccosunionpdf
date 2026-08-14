namespace DocumentManager.Core.Services;

public sealed class FinalPdfNameBuilder
{
    private static readonly HashSet<char> InvalidFileNameCharacters =
        Path.GetInvalidFileNameChars().Concat(['<', '>', ':', '"', '/', '\\', '|', '?', '*']).ToHashSet();

    public string Build(string economicNumber, string serviceOrderFolio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(economicNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceOrderFolio);

        var safeEconomicNumber = SanitizeSegment(economicNumber);
        var safeServiceOrderFolio = SanitizeSegment(serviceOrderFolio);
        return $"REPORTE MANTENIMIENTO EXTERNO{safeEconomicNumber} {safeServiceOrderFolio}.pdf";
    }

    public string SanitizeSegment(string value)
    {
        var sanitized = new string(value.Trim()
            .Select(character => InvalidFileNameCharacters.Contains(character) || char.IsControl(character)
                ? '_'
                : character)
            .ToArray())
            .Trim(' ', '.');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new ArgumentException("El folio no contiene caracteres válidos para un nombre de archivo.", nameof(value));
        }

        return sanitized;
    }
}
