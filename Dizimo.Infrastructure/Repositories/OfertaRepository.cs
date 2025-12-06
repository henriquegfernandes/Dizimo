using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Dizimo.Infrastructure.Repositories;

public class OfertaRepository : IOfertaRepository
{
    private readonly DizimoDbContext _context;
    public OfertaRepository(DizimoDbContext context) => _context = context;

    public async Task<Oferta?> GetByIdAsync(Guid id) => await _context.Ofertas.FindAsync(id);
    public async Task<IEnumerable<Oferta>> GetByDizimistaAsync(Guid dizimistaId) => await _context.Ofertas.Where(o => o.DizimistaId == dizimistaId).ToListAsync();
    public async Task<IEnumerable<Oferta>> GetByDateAsync(DateTime date) => await _context.Ofertas.Where(o => o.Data.Date == date.Date).ToListAsync();
    public async Task<IEnumerable<Oferta>> SearchAsync(DateTime? date, Guid? dizimistaId)
    {
        var query = _context.Ofertas.AsQueryable();
        if (date.HasValue) query = query.Where(o => o.Data.Date == date.Value.Date);
        if (dizimistaId.HasValue) query = query.Where(o => o.DizimistaId == dizimistaId.Value);
        return await query.ToListAsync();
    }
    public async Task<IEnumerable<Oferta>> GetAllAsync() => await _context.Ofertas.ToListAsync();
    public async Task AddAsync(Oferta oferta) { await _context.Ofertas.AddAsync(oferta); }
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
}
