using DocumentManager.Core.Models;
using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class ExpedientValidatorTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public ExpedientValidatorTests() => Directory.CreateDirectory(temporaryDirectory);

    [Fact]
    public void Validate_AcceptsCompleteExpedient()
    {
        var documents = CreateDocuments();
        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsEmptyFolioAndMissingDocuments()
    {
        var result = new ExpedientValidator().Validate(" ", " ", []);
        Assert.False(result.IsValid);
        Assert.Equal(6, result.Errors.Count);
    }

    private IReadOnlyList<DocumentInput> CreateDocuments() =>
        DocumentOrder.Required.Select(type =>
        {
            var path = Path.Combine(temporaryDirectory, $"{type}.pdf");
            File.WriteAllText(path, "test");
            return new DocumentInput(type, path);
        }).ToArray();

    public void Dispose() => Directory.Delete(temporaryDirectory, recursive: true);
}
