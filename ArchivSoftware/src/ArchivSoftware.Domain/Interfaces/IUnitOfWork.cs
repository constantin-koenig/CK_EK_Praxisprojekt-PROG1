namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Unit of Work Interface für transaktionale Operationen.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDocumentRepository Documents { get; }
    IFolderRepository Folders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
