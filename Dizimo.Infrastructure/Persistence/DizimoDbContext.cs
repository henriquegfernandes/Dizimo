using Dizimo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dizimo.Infrastructure.Persistence;

public class DizimoDbContext : DbContext
{
    public DizimoDbContext(DbContextOptions<DizimoDbContext> options) : base(options)
    {
    }

    public DbSet<Dizimista> Dizimistas { get; set; }
    public DbSet<Oferta> Ofertas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar Endereco como owned type com todas as propriedades necess�rias
        modelBuilder.Entity<Dizimista>().OwnsOne(d => d.Endereco, enderecoBuilder =>
        {
            enderecoBuilder.Property(e => e.Rua).IsRequired();
            enderecoBuilder.Property(e => e.Numero).IsRequired();
            enderecoBuilder.Property(e => e.Complemento).IsRequired();
            enderecoBuilder.Property(e => e.Bairro).IsRequired();
            enderecoBuilder.Property(e => e.Cidade).IsRequired();
            enderecoBuilder.Property(e => e.UF).IsRequired();
            enderecoBuilder.Property(e => e.CEP).IsRequired();
        });
    }
}