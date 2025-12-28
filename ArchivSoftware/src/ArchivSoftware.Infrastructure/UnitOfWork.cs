using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Infrastructure.Repositories;

namespace ArchivSoftware.Infrastructure;

/// <summary>
/// Unit of Work Implementierung für transaktionale Operationen.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ArchivSoftwareDbContext _context;
    private IDocumentRepository? _documentRepository;
    private IFolderRepository? _folderRepository;
    private bool _disposed;

    public UnitOfWork(ArchivSoftwareDbContext context)
    {
        _context = context;
    }

    public IDocumentRepository Documents =>
        _documentRepository ??= new DocumentRepository(_context);

    public IFolderRepository Folders =>
        _folderRepository ??= new FolderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
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
            _context.Dispose();
        }
        _disposed = true;
    }
}
