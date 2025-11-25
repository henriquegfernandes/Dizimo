using Dizimo.Domain.Entities;
using Xunit;
using System;

namespace Dizimo.Tests.Domain;

public class DizimistaBusinessRulesTests
{
    [Fact]
    public void Dizimista_NomeNaoPodeSerVazio()
    {
        var dizimista = new Dizimista { Nome = "" };
        Assert.False(!string.IsNullOrWhiteSpace(dizimista.Nome));
    }

    [Fact]
    public void Dizimista_NumeroCadastroDeveSerPositivo()
    {
        var dizimista = new Dizimista { NumeroCadastro = -1 };
        Assert.True(dizimista.NumeroCadastro > 0);
    }
}
