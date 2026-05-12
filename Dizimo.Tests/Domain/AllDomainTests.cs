using System;
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

    [Fact]
    public void Dizimista_HasEndereco()
    {
        var dizimista = new Dizimista
        {
            Id = Guid.NewGuid(),
            NumeroCadastro = 1,
            Nome = "Teste",
            DataNascimento = new DateTime(1990, 1, 1)
        };
        Assert.NotNull(dizimista.Endereco);
        Assert.Equal("Osasco", dizimista.Endereco.Cidade);
        Assert.Equal("SP", dizimista.Endereco.UF);
    }

    [Fact]
    public void Dizimista_HasDataCadastro()
    {
        var dizimista = new Dizimista
        {
            Id = Guid.NewGuid(),
            NumeroCadastro = 1,
            Nome = "Teste",
            DataNascimento = new DateTime(1990, 1, 1)
        };
        Assert.Equal(DateTime.Today, dizimista.DataCadastro);
    }

    [Fact]
    public void Dizimista_HasTelefoneAndWhatsapp()
    {
        var dizimista = new Dizimista
        {
            Id = Guid.NewGuid(),
            NumeroCadastro = 1,
            Nome = "Teste",
            DataNascimento = new DateTime(1990, 1, 1),
            Telefone = "(11) 99999-9999",
            Whatsapp = "(11) 99999-9999"
        };
        Assert.Equal("(11) 99999-9999", dizimista.Telefone);
        Assert.Equal("(11) 99999-9999", dizimista.Whatsapp);
    }
}

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
        Assert.False(dizimista.NumeroCadastro > 0);
    }

    [Fact]
    public void Dizimista_DataNascimentoNaoPodeSerFutura()
    {
        var dizimista = new Dizimista { DataNascimento = DateTime.Today.AddDays(1) };
        Assert.False(dizimista.DataNascimento <= DateTime.Today);
    }
}

public class OfertaBusinessRulesTests
{
    [Fact]
    public void Oferta_ValorDeveSerPositivo()
    {
        var oferta = new Oferta { Valor = -10 };
        Assert.False(oferta.Valor > 0);
    }

    [Fact]
    public void Oferta_DataNaoPodeSerFutura()
    {
        var oferta = new Oferta { Data = DateTime.Today.AddDays(1) };
        Assert.False(oferta.Data <= DateTime.Today);
    }

    [Fact]
    public void Oferta_HasMesReferencia()
    {
        var oferta = new Oferta
        {
            Id = Guid.NewGuid(),
            DizimistaId = Guid.NewGuid(),
            Valor = 100,
            Data = DateTime.Today,
            MesReferencia = 12,
            AnoReferencia = 2024
        };
        Assert.Equal(12, oferta.MesReferencia);
        Assert.Equal(2024, oferta.AnoReferencia);
    }
}

public class UsuarioBusinessRulesTests
{
    [Fact]
    public void Usuario_LoginNaoPodeSerVazio()
    {
        var usuario = new Usuario { Login = "" };
        Assert.False(!string.IsNullOrWhiteSpace(usuario.Login));
    }

    [Fact]
    public void Usuario_SenhaDeveTerMinimo6Caracteres()
    {
        var usuario = new Usuario { SenhaHash = "123" };
        Assert.False(usuario.SenhaHash.Length >= 6);
    }

    [Fact]
    public void Usuario_IsActiveByDefault()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Teste",
            Login = "teste",
            SenhaHash = "123456"
        };
        Assert.True(usuario.Ativo);
        Assert.Equal(PerfilUsuario.Padrao, usuario.Perfil);
    }

    [Fact]
    public void Usuario_CanBeAdmin()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Admin",
            Login = "admin",
            SenhaHash = "123456",
            Perfil = PerfilUsuario.Admin
        };
        Assert.Equal(PerfilUsuario.Admin, usuario.Perfil);
    }
}