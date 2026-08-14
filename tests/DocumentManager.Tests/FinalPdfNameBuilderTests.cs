using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class FinalPdfNameBuilderTests
{
    [Theory]
    [InlineData("123", "OS-12345", "REPORTE MANTENIMIENTO EXTERNO123 OS-12345.pdf")]
    [InlineData(" ECO/45 ", " OS:12/34 ", "REPORTE MANTENIMIENTO EXTERNOECO_45 OS_12_34.pdf")]
    public void Build_SanitizesFileName(string economicNumber, string serviceOrderFolio, string expected)
    {
        var sut = new FinalPdfNameBuilder();
        Assert.Equal(expected, sut.Build(economicNumber, serviceOrderFolio));
    }
}
