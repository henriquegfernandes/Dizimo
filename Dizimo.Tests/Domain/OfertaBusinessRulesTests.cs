using Dizimo.Domain.Entities;
using Xunit;
using System;

namespace Dizimo.Tests.Domain;

public class OfertaBusinessRulesTests
{
    [Fact]
    public void Oferta_ValorDeveSerPositivo()
    {
        var oferta = new Oferta { Valor = -10 };
        Assert.True(oferta.Valor > 0);
    }

    [Fact]
    public void Oferta_DataNaoPodeSerFutura()
    {
        var oferta = new Oferta { Data = DateTime.Today.AddDays(1) };
        Assert.True(oferta.Data <= DateTime.Today);
    }
}
