using Dizimo.Domain.Entities;
using Dizimo.Domain.Models;
using Dizimo.Domain.Repositories;
using Dizimo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dizimo.Infrastructure.Repositories;

public class OfertaRepository : IOfertaRepository
{
    private readonly DizimoDbContext _context;

    public OfertaRepository(DizimoDbContext context)
    {
        _context = context;
    }

    public async Task<Oferta?> GetByIdAsync(Guid id)
    {
        return await _context.Ofertas.FindAsync(id);
    }

    public async Task<IEnumerable<Oferta>> GetByDizimistaAsync(Guid dizimistaId)
    {
        return await _context.Ofertas.Where(o => o.DizimistaId == dizimistaId).ToListAsync();
    }

    public async Task<IEnumerable<Oferta>> GetByDateAsync(DateTime date)
    {
        return await _context.Ofertas.Where(o => o.Data.Date == date.Date).ToListAsync();
    }

    public async Task<IEnumerable<Oferta>> SearchAsync(DateTime? date, Guid? dizimistaId)
    {
        var query = _context.Ofertas.AsQueryable();
        if (date.HasValue) query = query.Where(o => o.Data.Date == date.Value.Date);
        if (dizimistaId.HasValue) query = query.Where(o => o.DizimistaId == dizimistaId.Value);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Oferta>> GetAllAsync()
    {
        return await _context.Ofertas.ToListAsync();
    }

    public async Task<decimal> GetTotalValorAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
        string? tipoPagamento = null, string? filtroNome = null)
    {
        var query = _context.Ofertas.AsQueryable();

        // Aplicar filtros de data no SQL
        if (dataInicio.HasValue)
            query = query.Where(o => o.Data.Date >= dataInicio.Value.Date);

        if (dataFim.HasValue)
            query = query.Where(o => o.Data.Date <= dataFim.Value.Date);

        // Aplicar filtro de tipo de pagamento no SQL
        if (!string.IsNullOrWhiteSpace(tipoPagamento) && tipoPagamento != "Todos")
        {
            var tipoPagamentoNormalizado = tipoPagamento.Replace("Cartão", "Cartao");
            if (Enum.TryParse<TipoPagamento>(tipoPagamentoNormalizado, out var tipo))
                query = query.Where(o => o.TipoPagamento == tipo);
        }

        // Aplicar filtro de nome/código do dizimista
        query = ApplyNomeFilter(query, filtroNome);

        return await query.SumAsync(o => o.Valor);
    }

    public async Task<PaginatedResult<Oferta>> GetAllPaginatedAsync(int pageNumber, int pageSize,
        DateTime? dataInicio = null, DateTime? dataFim = null, string? tipoPagamento = null, string? filtroNome = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.Ofertas.AsQueryable();

        // Aplicar filtros de data no SQL
        if (dataInicio.HasValue)
            query = query.Where(o => o.Data.Date >= dataInicio.Value.Date);

        if (dataFim.HasValue)
            query = query.Where(o => o.Data.Date <= dataFim.Value.Date);

        // Aplicar filtro de tipo de pagamento no SQL
        if (!string.IsNullOrWhiteSpace(tipoPagamento) && tipoPagamento != "Todos")
        {
            var tipoPagamentoNormalizado = tipoPagamento.Replace("Cartão", "Cartao");
            if (Enum.TryParse<TipoPagamento>(tipoPagamentoNormalizado, out var tipo))
                query = query.Where(o => o.TipoPagamento == tipo);
        }

        // Aplicar filtro de nome/código do dizimista
        query = ApplyNomeFilter(query, filtroNome);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.Data)
            .ThenBy(o => o.DizimistaId)
            .ThenBy(o => o.AnoReferencia)
            .ThenBy(o => o.MesReferencia)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Oferta>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(Oferta oferta)
    {
        await _context.Ofertas.AddAsync(oferta);
    }

    public async Task UpdateAsync(Oferta oferta)
    {
        var existingOferta = await _context.Ofertas.FindAsync(oferta.Id);
        if (existingOferta != null)
        {
            existingOferta.DizimistaId = oferta.DizimistaId;
            existingOferta.Valor = oferta.Valor;
            existingOferta.Data = oferta.Data;
            existingOferta.MesReferencia = oferta.MesReferencia;
            existingOferta.AnoReferencia = oferta.AnoReferencia;
            _context.Ofertas.Update(existingOferta);
        }

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Ofertas.FindAsync(id);
        if (entity != null) _context.Ofertas.Remove(entity);
    }

    /// <summary>
    ///     Aplica filtro case-insensitive por nome ou número de cadastro do dizimista.
    ///     Trata NumeroCadastro com tentativa de parse numérico para melhor performance.
    /// </summary>
    private IQueryable<Oferta> ApplyNomeFilter(IQueryable<Oferta> query, string? filtroNome)
    {
        if (string.IsNullOrWhiteSpace(filtroNome))
            return query;

        // Tenta fazer parse do filtro como número para comparação numérica
        var isNumericFilter = int.TryParse(filtroNome, out var numeroCadastro);

        // Converte para minúsculas para comparação case-insensitive
        var filtroMinusculo = filtroNome.ToLower();

        query = query.Join(
                _context.Dizimistas,
                oferta => oferta.DizimistaId,
                dizimista => dizimista.Id,
                (oferta, dizimista) => new { oferta, dizimista }
            )
            .Where(x =>
                // Filtro por nome (case-insensitive)
                x.dizimista.Nome.ToLower().Contains(filtroMinusculo) ||
                // Se for número, tenta comparação numérica exata; caso contrário, tenta partial match
                (isNumericFilter
                    ? x.dizimista.NumeroCadastro == numeroCadastro
                    : x.dizimista.NumeroCadastro.ToString().ToLower().Contains(filtroMinusculo))
            )
            .Select(x => x.oferta);

        return query;
    }
}