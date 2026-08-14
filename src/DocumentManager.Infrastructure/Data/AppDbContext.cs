using DocumentManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();

    public DbSet<FolioSequence> FolioSequences => Set<FolioSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var record = modelBuilder.Entity<ServiceRecord>();
        record.ToTable("ServiceRecords");
        record.HasKey(item => item.Id);
        record.Property(item => item.ServiceOrderFolio).HasMaxLength(100).IsRequired();
        record.Property(item => item.InternalFolio).HasMaxLength(32).IsRequired();
        record.Property(item => item.FinalPdfPath).HasMaxLength(2048).IsRequired();
        record.HasIndex(item => item.InternalFolio).IsUnique();
        record.HasIndex(item => item.ServiceOrderFolio);
        record.HasIndex(item => item.Date);

        var sequence = modelBuilder.Entity<FolioSequence>();
        sequence.ToTable("FolioSequences");
        sequence.HasKey(item => item.Id);
        sequence.Property(item => item.LastValue).IsRequired();
    }
}
