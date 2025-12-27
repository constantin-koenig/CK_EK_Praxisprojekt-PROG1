using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Infrastructure.Repositories;

namespace ArchivSoftware.Infrastructure;

/// <summary>
/// Unit of Work Implementierung für transaktionale Operationen.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ArchivDbContext _context;
    private IDocumentRepository? _documentRepository;
    private ICategoryRepository? _categoryRepository;
    private ITagRepository? _tagRepository;
    private bool _disposed;

    public UnitOfWork(ArchivDbContext context)
    {
        _context = context;
    }

    public IDocumentRepository Documents =>
        _documentRepository ??= new DocumentRepository(_context);

    public ICategoryRepository Categories =>
        _categoryRepository ??= new CategoryRepository(_context);

    public ITagRepository Tags =>
        _tagRepository ??= new TagRepository(_context);

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
