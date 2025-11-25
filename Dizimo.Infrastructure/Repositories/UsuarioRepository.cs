using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Dizimo.Infrastructure.Persistence;

namespace Dizimo.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DizimoDbContext _context;
    public UsuarioRepository(DizimoDbContext context) => _context = context;

    public async Task<Usuario?> GetByIdAsync(Guid id) => await _context.Usuarios.FindAsync(id);
    public async Task<Usuario?> GetByLoginAsync(string login) => await _context.Usuarios.FirstOrDefaultAsync(u => u.Login == login);
    public async Task<IEnumerable<Usuario>> GetAllAsync() => await _context.Usuarios.ToListAsync();
    public async Task AddAsync(Usuario usuario) { await _context.Usuarios.AddAsync(usuario); }
    public async Task UpdateAsync(Usuario usuario) { _context.Usuarios.Update(usuario); await Task.CompletedTask; }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Usuarios.FindAsync(id);
        if (entity != null) _context.Usuarios.Remove(entity);
    }
    public async Task InativarAsync(Guid id)
    {
        var entity = await _context.Usuarios.FindAsync(id);
        if (entity != null) { entity.Ativo = false; _context.Usuarios.Update(entity); }
    }
}
