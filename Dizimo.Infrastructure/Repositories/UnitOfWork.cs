using System.Threading.Tasks;
using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Persistence;

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

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
