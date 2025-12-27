using ArchivSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext für die ArchivSoftware.
/// </summary>
public class ArchiveDbContext : DbContext
{
    public ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : base(options)
    {
    }

    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Folder Configuration
        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Self-referencing relation: ParentFolder (optional) / Children
            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.Children)
                .HasForeignKey(e => e.ParentFolderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Index: (ParentFolderId, Name) muss eindeutig sein
            entity.HasIndex(e => new { e.ParentFolderId, e.Name }).IsUnique();
        });

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Pflichtfelder mit MaxLength
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Data).IsRequired().HasColumnType("varbinary(max)");
            entity.Property(e => e.PlainText).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.Sha256).HasMaxLength(64);
            entity.Property(e => e.CreatedAt).IsRequired();

            // FolderId FK - Restrict: Folder darf nicht gelöscht werden, wenn Documents existieren
            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Documents)
                .HasForeignKey(e => e.FolderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Indizes für Performance
            entity.HasIndex(e => e.Sha256);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
