using Dizimo.Domain.Entities;
using Xunit;

namespace Dizimo.Tests.Domain;

public class DizimistaTests
{
    [Fact]
    public void Dizimista_Created_IsActiveByDefault()
    {
        var dizimista = new Dizimista
        {
            Id = Guid.NewGuid(),
            NumeroCadastro = 1,
            Nome = "Teste",
            DataNascimento = new DateTime(1990, 1, 1)
        };
        Assert.True(dizimista.Ativo);
    }

    [Fact]
    public void Dizimista_CanBeInactivated()
    {
        var dizimista = new Dizimista { Ativo = true };
        dizimista.Ativo = false;
        Assert.False(dizimista.Ativo);
    }
}
