using Microsoft.EntityFrameworkCore;
using Dizimo.Domain.Entities;

namespace Dizimo.Infrastructure.Persistence;

public class DizimoDbContext : DbContext
{
    public DbSet<Dizimista> Dizimistas { get; set; }
    public DbSet<Oferta> Ofertas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    public DizimoDbContext(DbContextOptions<DizimoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Dizimista>().OwnsOne(d => d.Endereco);
    }
}
