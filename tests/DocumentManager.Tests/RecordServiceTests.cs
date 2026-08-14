using DocumentManager.Core.Models;
using DocumentManager.Infrastructure.Data;
using DocumentManager.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Tests;

public sealed class RecordServiceTests : IAsyncLifetime
{
    private readonly string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private RecordService service = null!;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(temporaryDirectory);
        var databasePath = Path.Combine(temporaryDirectory, "records.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        service = new RecordService(new TestDbContextFactory(options));
        await service.InitializeAsync();
    }

    [Fact]
    public async Task GetNextInternalFolioAsync_DoesNotConsumeSequence()
    {
        Assert.Equal("EXP-000001", await service.GetNextInternalFolioAsync());
        Assert.Equal("EXP-000001", await service.GetNextInternalFolioAsync());
        Assert.Empty(await service.SearchAsync(null));
    }

    [Fact]
    public async Task CreateWithNextInternalFolioAsync_IncrementsSequenceAndCreatesRecordAtomically()
    {
        var first = await service.CreateWithNextInternalFolioAsync(
            new DateTime(2026, 8, 13),
            "OS-100",
            "C:\\Expedientes\\primero.pdf");

        Assert.Equal("EXP-000001", first.InternalFolio);
        Assert.Equal("EXP-000002", await service.GetNextInternalFolioAsync());

        var second = await service.CreateWithNextInternalFolioAsync(
            new DateTime(2026, 8, 14),
            "OS-200",
            "C:\\Expedientes\\segundo.pdf");

        Assert.Equal("EXP-000002", second.InternalFolio);
        Assert.Equal(2, (await service.SearchAsync(null)).Count);
    }

    [Fact]
    public async Task SearchAsync_FindsBothFolioTypesAndSortsNewestFirst()
    {
        await service.CreateWithNextInternalFolioAsync(
            new DateTime(2026, 8, 13),
            "OS-100",
            "C:\\Expedientes\\primero.pdf");
        await service.CreateWithNextInternalFolioAsync(
            new DateTime(2026, 8, 14),
            "OS-200",
            "C:\\Expedientes\\segundo.pdf");

        var all = await service.SearchAsync(null);
        var byInternal = await service.SearchAsync("000001");
        var byServiceOrder = await service.SearchAsync("os-200");

        Assert.Equal("EXP-000002", all[0].InternalFolio);
        Assert.Single(byInternal);
        Assert.Equal("OS-100", byInternal[0].ServiceOrderFolio);
        Assert.Single(byServiceOrder);
        Assert.Equal("EXP-000002", byServiceOrder[0].InternalFolio);
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(temporaryDirectory, recursive: true);
        return Task.CompletedTask;
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
