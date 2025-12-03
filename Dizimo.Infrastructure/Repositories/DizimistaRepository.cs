using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dizimo.Infrastructure.Repositories;

public class DizimistaRepository : IDizimistaRepository
{
    private readonly DizimoDbContext _context;
    public DizimistaRepository(DizimoDbContext context) => _context = context;

    public async Task<Dizimista?> GetByIdAsync(Guid id) => 
        await _context.Dizimistas
            .Include(d => d.Endereco)
            .FirstOrDefaultAsync(d => d.Id == id);
            
    public async Task<Dizimista?> GetByNumeroCadastroAsync(int numeroCadastro) => 
        await _context.Dizimistas
            .Include(d => d.Endereco)
            .FirstOrDefaultAsync(d => d.NumeroCadastro == numeroCadastro);
            
    public async Task<IEnumerable<Dizimista>> GetAllAsync() => 
        await _context.Dizimistas
            .Include(d => d.Endereco)
            .ToListAsync();
            
    public async Task<IEnumerable<Dizimista>> GetAniversariantesAsync(int mes) => 
        await _context.Dizimistas
            .Include(d => d.Endereco)
            .Where(d => d.DataNascimento.Month == mes)
            .ToListAsync();
            
    public async Task AddAsync(Dizimista dizimista) { await _context.Dizimistas.AddAsync(dizimista); }
    
    public async Task UpdateAsync(Dizimista dizimista)
    {
        var entity = await _context.Dizimistas
            .Include(d => d.Endereco)
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
