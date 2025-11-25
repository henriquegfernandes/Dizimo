using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dizimo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DizimoDbContext _context;
    public IDizimistaRepository Dizimistas { get; }
    public IOfertaRepository Ofertas { get; }
    public IUsuarioRepository Usuarios { get; }

    public UnitOfWork(DizimoDbContext context)
    {
        _context = context;
        Dizimistas = new DizimistaRepository(_context);
        Ofertas = new OfertaRepository(_context);
        Usuarios = new UsuarioRepository(_context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
