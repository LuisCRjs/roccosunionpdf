using DocumentManager.Core.Models;
using DocumentManager.Infrastructure.Data;
using DocumentManager.Infrastructure.Services;
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
    public async Task ReserveNextInternalFolioAsync_IsMonotonicAndDoesNotUseRecordCount()
    {
        Assert.Equal("EXP-000001", await service.ReserveNextInternalFolioAsync());
        Assert.Equal("EXP-000002", await service.ReserveNextInternalFolioAsync());
        Assert.Empty(await service.SearchAsync(null));
        Assert.Equal("EXP-000003", await service.ReserveNextInternalFolioAsync());
    }

    [Fact]
    public async Task SearchAsync_FindsBothFolioTypesAndSortsNewestFirst()
    {
        await service.CreateAsync(CreateRecord("EXP-000010", "OS-100", new DateTime(2026, 8, 13)));
        await service.CreateAsync(CreateRecord("EXP-000011", "OS-200", new DateTime(2026, 8, 14)));

        var all = await service.SearchAsync(null);
        var byInternal = await service.SearchAsync("000010");
        var byServiceOrder = await service.SearchAsync("os-200");

        Assert.Equal("EXP-000011", all[0].InternalFolio);
        Assert.Single(byInternal);
        Assert.Equal("OS-100", byInternal[0].ServiceOrderFolio);
        Assert.Single(byServiceOrder);
        Assert.Equal("EXP-000011", byServiceOrder[0].InternalFolio);
    }

    private static ServiceRecord CreateRecord(string internalFolio, string osFolio, DateTime date) => new()
    {
        Date = date,
        InternalFolio = internalFolio,
        ServiceOrderFolio = osFolio,
        FinalPdfPath = $"C:\\Expedientes\\{internalFolio}.pdf",
    };

    public Task DisposeAsync()
    {
        Directory.Delete(temporaryDirectory, recursive: true);
        return Task.CompletedTask;
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
