using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure;

/// <summary>
/// Factory für die Erstellung von UnitOfWork-Instanzen.
/// Nutzt die DbContextFactory für Mandanten-Support.
/// </summary>
public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<ArchivSoftwareDbContext> _contextFactory;

    public UnitOfWorkFactory(IDbContextFactory<ArchivSoftwareDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IUnitOfWork Create()
    {
        return new UnitOfWork(_contextFactory);
    }
}
