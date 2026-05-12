using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dizimo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DizimoDbContext _context;

    public UnitOfWork(DizimoDbContext context)
    {
        _context = context;
        Dizimistas = new DizimistaRepository(_context);
        Ofertas = new OfertaRepository(_context);
        Usuarios = new UsuarioRepository(_context);
    }

    public IDizimistaRepository Dizimistas { get; }
    public IOfertaRepository Ofertas { get; }
    public IUsuarioRepository Usuarios { get; }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task ClearDbContextAsync()
    {
        // Descarta todas as entidades rastreadas para garantir que o pr�ximo acesso seja do banco
        foreach (var entry in _context.ChangeTracker.Entries()) entry.State = EntityState.Detached;
        await Task.CompletedTask;
    }
}