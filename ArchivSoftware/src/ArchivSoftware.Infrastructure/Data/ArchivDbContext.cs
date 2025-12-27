using ArchivSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext für die ArchivSoftware.
/// </summary>
public class ArchivDbContext : DbContext
{
    public ArchivDbContext(DbContextOptions<ArchivDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Folder> Folders => Set<Folder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Folder Configuration
        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.Children)
                .HasForeignKey(e => e.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.Name, e.ParentFolderId }).IsUnique();
        });

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.PlainText);
            entity.Property(e => e.Sha256).HasMaxLength(64);

            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Documents)
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Sha256);
            entity.HasIndex(e => e.FolderId);
        });
    }
}
