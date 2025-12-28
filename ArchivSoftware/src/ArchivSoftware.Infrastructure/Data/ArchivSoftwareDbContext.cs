using ArchivSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext für die ArchivSoftware.
/// </summary>
public class ArchivSoftwareDbContext : DbContext
{
    public ArchivSoftwareDbContext(DbContextOptions<ArchivSoftwareDbContext> options) : base(options)
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
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Self-referencing relationship: 1 Folder has many Children
            // SQL Server does not support cascade delete on self-referencing relationships
            // Deletion of subfolders must be handled in application code
            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.Children)
                .HasForeignKey(e => e.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Index: (ParentFolderId, Name) - Ordnernamen pro Ebene eindeutig
            entity.HasIndex(e => new { e.ParentFolderId, e.Name })
                .IsUnique();
        });

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(260);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            // Data as varbinary(max)
            entity.Property(e => e.Data)
                .IsRequired()
                .HasColumnType("varbinary(max)");

            // PlainText as nvarchar(max)
            entity.Property(e => e.PlainText)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Sha256)
                .HasMaxLength(64);

            // 1 Folder has many Documents - Cascade delete
            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Documents)
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Sha256);
            entity.HasIndex(e => e.FolderId);
        });
    }
}
