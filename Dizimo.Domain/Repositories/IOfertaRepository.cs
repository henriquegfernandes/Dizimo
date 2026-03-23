using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Models;

namespace Dizimo.Domain.Repositories;

public interface IOfertaRepository
{
    Task<Oferta?> GetByIdAsync(Guid id);
    Task<IEnumerable<Oferta>> GetByDizimistaAsync(Guid dizimistaId);
    Task<IEnumerable<Oferta>> GetByDateAsync(DateTime date);
    Task<IEnumerable<Oferta>> SearchAsync(DateTime? date, Guid? dizimistaId);
    Task<IEnumerable<Oferta>> GetAllAsync();
    Task<decimal> GetTotalValorAsync(DateTime? dataInicio = null, DateTime? dataFim = null, string? tipoPagamento = null, string? filtroNome = null);
    Task<PaginatedResult<Oferta>> GetAllPaginatedAsync(int pageNumber, int pageSize, DateTime? dataInicio = null, DateTime? dataFim = null, string? tipoPagamento = null, string? filtroNome = null);
    Task AddAsync(Oferta oferta);
    Task UpdateAsync(Oferta oferta);
    Task DeleteAsync(Guid id);
}
