using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dizimo.Domain.Models;

namespace Dizimo.Infrastructure.Repositories;

public class DizimistaRepository : IDizimistaRepository
{
    private readonly DizimoDbContext _context;
    public DizimistaRepository(DizimoDbContext context) => _context = context;

    public async Task<Dizimista?> GetByIdAsync(Guid id) => 
        await _context.Dizimistas
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
            
    public async Task<Dizimista?> GetByNumeroCadastroAsync(int numeroCadastro) => 
        await _context.Dizimistas
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.NumeroCadastro == numeroCadastro);
            
    public async Task<IEnumerable<Dizimista>> GetAllAsync() => 
        await _context.Dizimistas
            .AsNoTracking()
            .ToListAsync();

    public async Task<PaginatedResult<Dizimista>> GetAllPaginatedAsync(int pageNumber, int pageSize, string? filtroNome = null, string? statusSelecionado = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.Dizimistas.AsNoTracking().AsQueryable();

        // Aplicar filtro de status no SQL (pode ser traduzido)
        if (!string.IsNullOrWhiteSpace(statusSelecionado) && statusSelecionado != "Todos")
        {
            if (statusSelecionado == "Ativos")
                query = query.Where(d => d.Ativo);
            else if (statusSelecionado == "Inativos")
                query = query.Where(d => !d.Ativo);
        }

        // Se há filtro de nome, precisa trazer para cliente e contar depois
        // Caso contrário, pode fazer tudo no SQL
        PaginatedResult<Dizimista> result;

        if (string.IsNullOrWhiteSpace(filtroNome))
        {
            // Sem filtro de nome: fazer tudo no SQL (mais eficiente)
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            result = new PaginatedResult<Dizimista>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        else
        {
            // Com filtro de nome: precisa trazer para cliente
            var itemsFromDb = await query
                .OrderBy(d => d.Nome)
                .ToListAsync();

            var filteredItems = itemsFromDb.Where(d =>
                d.Nome.Contains(filtroNome, StringComparison.OrdinalIgnoreCase) ||
                d.NumeroCadastro.ToString().Contains(filtroNome))
                .ToList();

            var totalCount = filteredItems.Count;
            var items = filteredItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            result = new PaginatedResult<Dizimista>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        return result;
    }
            
    public async Task<IEnumerable<Dizimista>> GetAniversariantesAsync(int mes) => 
        await _context.Dizimistas
            .AsNoTracking()
            .Where(d => d.DataNascimento.Month == mes)
            .ToListAsync();
            
    public async Task AddAsync(Dizimista dizimista) { await _context.Dizimistas.AddAsync(dizimista); }
    
    public async Task UpdateAsync(Dizimista dizimista)
    {
        var entity = await _context.Dizimistas
            .FirstOrDefaultAsync(d => d.Id == dizimista.Id);
        if (entity != null)
        {
            entity.NumeroCadastro = dizimista.NumeroCadastro;
            entity.Nome = dizimista.Nome;
            entity.DataNascimento = dizimista.DataNascimento;
            entity.Ativo = dizimista.Ativo;
            entity.Endereco.Rua = dizimista.Endereco.Rua;
            entity.Endereco.Numero = dizimista.Endereco.Numero;
            entity.Endereco.Complemento = dizimista.Endereco.Complemento;
            entity.Endereco.Bairro = dizimista.Endereco.Bairro;
            entity.Endereco.Cidade = dizimista.Endereco.Cidade;
            entity.Endereco.UF = dizimista.Endereco.UF;
            entity.Endereco.CEP = dizimista.Endereco.CEP;
            entity.Telefone = dizimista.Telefone;
            entity.Whatsapp = dizimista.Whatsapp;
            entity.DataCadastro = dizimista.DataCadastro;
            _context.Dizimistas.Update(entity);
        }
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Dizimistas.FindAsync(id);
        if (entity != null) _context.Dizimistas.Remove(entity);
    }
    
    public async Task InativarAsync(Guid id)
    {
        var entity = await _context.Dizimistas.FindAsync(id);
        if (entity != null) 
        { 
            entity.Ativo = !entity.Ativo;
            _context.Dizimistas.Update(entity); 
        }
    }
}


