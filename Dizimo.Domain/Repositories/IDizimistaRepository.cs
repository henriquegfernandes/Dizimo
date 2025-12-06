using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;

namespace Dizimo.Domain.Repositories;

public interface IDizimistaRepository
{
    Task<Dizimista?> GetByIdAsync(Guid id);
    Task<Dizimista?> GetByNumeroCadastroAsync(int numeroCadastro);
    Task<IEnumerable<Dizimista>> GetAllAsync();
    Task<IEnumerable<Dizimista>> GetAniversariantesAsync(int mes);
    Task AddAsync(Dizimista dizimista);
    Task UpdateAsync(Dizimista dizimista);
    Task DeleteAsync(Guid id);
    Task InativarAsync(Guid id);
}
