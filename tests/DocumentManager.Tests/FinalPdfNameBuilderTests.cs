using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class FinalPdfNameBuilderTests
{
    [Theory]
    [InlineData("EXP-000123", "OS-12345", "EXP-000123_OS-12345.pdf")]
    [InlineData("EXP-000123", " OS:12/34 ", "EXP-000123_OS_12_34.pdf")]
    public void Build_SanitizesFileName(string internalFolio, string serviceOrderFolio, string expected)
    {
        var sut = new FinalPdfNameBuilder();
        Assert.Equal(expected, sut.Build(internalFolio, serviceOrderFolio));
    }
}

