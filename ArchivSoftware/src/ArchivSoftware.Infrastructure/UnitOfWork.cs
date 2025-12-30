using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure;

/// <summary>
/// Unit of Work Implementierung für transaktionale Operationen.
/// Nutzt DbContextFactory für Mandanten-Support.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContextFactory<ArchivSoftwareDbContext> _contextFactory;
    private ArchivSoftwareDbContext? _context;
    private IDocumentRepository? _documentRepository;
    private IFolderRepository? _folderRepository;
    private bool _disposed;

    public UnitOfWork(IDbContextFactory<ArchivSoftwareDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Lazy-Initialisierter DbContext für die aktuelle Unit of Work.
    /// </summary>
    private ArchivSoftwareDbContext Context => _context ??= _contextFactory.CreateDbContext();

    public IDocumentRepository Documents =>
        _documentRepository ??= new DocumentRepository(Context);

    public IFolderRepository Folders =>
        _folderRepository ??= new FolderRepository(Context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
        }
        _disposed = true;
    }
}
