using Dizimo.Domain.Entities;
using Xunit;
using System;

namespace Dizimo.Tests.Domain;

public class UsuarioBusinessRulesTests
{
    [Fact]
    public void Usuario_LoginNaoPodeSerVazio()
    {
        var usuario = new Usuario { Login = "" };
        Assert.False(string.IsNullOrWhiteSpace(usuario.Login));
    }

    [Fact]
    public void Usuario_SenhaDeveTerMinimo6Caracteres()
    {
        var usuario = new Usuario { SenhaHash = "123" };
        Assert.True(usuario.SenhaHash.Length >= 6);
    }
}
