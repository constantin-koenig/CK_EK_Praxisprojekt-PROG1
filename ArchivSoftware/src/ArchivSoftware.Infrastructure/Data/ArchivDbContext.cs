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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Tags)
                .WithMany(t => t.Documents)
                .UsingEntity(j => j.ToTable("DocumentTags"));
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Tag Configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
