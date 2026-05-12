using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Models;

namespace Dizimo.Domain.Repositories;

public interface IDizimistaRepository
{
    Task<Dizimista?> GetByIdAsync(Guid id);
    Task<Dizimista?> GetByNumeroCadastroAsync(int numeroCadastro);
    Task<IEnumerable<Dizimista>> GetAllAsync();
    Task<IEnumerable<Dizimista>> GetAniversariantesAsync(int mes);

    Task<PaginatedResult<Dizimista>> GetAllPaginatedAsync(int pageNumber, int pageSize, string? filtroNome = null,
        string? statusSelecionado = null);

    Task AddAsync(Dizimista dizimista);
    Task UpdateAsync(Dizimista dizimista);
    Task DeleteAsync(Guid id);
    Task InativarAsync(Guid id);
}