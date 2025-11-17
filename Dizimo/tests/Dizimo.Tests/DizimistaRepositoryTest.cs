using System.IO;
using System.Threading.Tasks;
using Xunit;
using Dizimo.Core.Data;
using Dizimo.Core.Models;

public class DizimistaRepositoryTest
{
    [Fact]
    public async Task CanCreateAndListDizimista()
    {
        // arrange
        var tempDb = Path.Combine(Path.GetTempPath(), "test_db_repo.db3");
        if (File.Exists(tempDb)) File.Delete(tempDb);

        var repo = new DizimistaRepository(tempDb);
        await repo.InitAsync();

        // act
        var id = await repo.SaveAsync(new Dizimista { Nome = "Teste", Codigo = "999" });
        var list = await repo.ListAsync();

        // assert
        Assert.True(id > 0);
        Assert.Contains(list, d => d.Codigo == "999");
    }
}
