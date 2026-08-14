namespace DocumentManager.Core.Services;

public static class FolioFormatter
{
    public static string Format(long sequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        return $"EXP-{sequence:D6}";
    }
}

