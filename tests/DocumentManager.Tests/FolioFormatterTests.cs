using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class FolioFormatterTests
{
    [Theory]
    [InlineData(1, "EXP-000001")]
    [InlineData(123, "EXP-000123")]
    [InlineData(1_000_000, "EXP-1000000")]
    public void Format_UsesExpectedPrefixAndMinimumWidth(long sequence, string expected)
    {
        Assert.Equal(expected, FolioFormatter.Format(sequence));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Format_RejectsNonPositiveValues(long sequence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.Format(sequence));
    }
}

