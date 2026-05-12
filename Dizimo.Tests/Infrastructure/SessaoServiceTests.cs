using Dizimo.Services;
using Xunit;

namespace Dizimo.Tests.Infrastructure;

public class SessaoServiceTests
{
    [Fact]
    public void HashSenha_ReturnsConsistentHash()
    {
        var senha = "123456";
        var hash1 = SessaoService.HashSenha(senha);
        var hash2 = SessaoService.HashSenha(senha);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashSenha_DifferentPasswords_ReturnDifferentHashes()
    {
        var hash1 = SessaoService.HashSenha("senha1");
        var hash2 = SessaoService.HashSenha("senha2");
        Assert.NotEqual(hash1, hash2);
    }
}