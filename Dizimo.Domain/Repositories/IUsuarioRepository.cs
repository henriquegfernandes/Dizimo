using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;

namespace Dizimo.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByLoginAsync(string login);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task DeleteAsync(Guid id);
}