using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dizimo.Domain.Repositories;

public interface IOfertaRepository
{
    Task<Oferta?> GetByIdAsync(Guid id);
    Task<IEnumerable<Oferta>> GetByDizimistaAsync(Guid dizimistaId);
    Task<IEnumerable<Oferta>> GetByDateAsync(DateTime date);
    Task<IEnumerable<Oferta>> SearchAsync(DateTime? date, Guid? dizimistaId);
    Task<IEnumerable<Oferta>> GetAllAsync();
    Task AddAsync(Oferta oferta);
    Task UpdateAsync(Oferta oferta);
    Task DeleteAsync(Guid id);
}
