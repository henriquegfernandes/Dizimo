using System.Threading.Tasks;

namespace Dizimo.Domain.Repositories;

public interface IUnitOfWork
{
    IDizimistaRepository Dizimistas { get; }
    IOfertaRepository Ofertas { get; }
    IUsuarioRepository Usuarios { get; }
    Task<int> SaveChangesAsync();
}
