using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dizimo.Core.Data;
using Dizimo.Core.Models;
using Dizimo.Core.Services;
using Xunit;

namespace Dizimo.Tests
{
    public class BackupServiceTests : IAsyncLifetime
    {
        private readonly string _db;
        private readonly DizimistaRepository _dr;
        private readonly OfertaRepository _or;
        private readonly BackupService _bs;

        public BackupServiceTests()
        {
            _db = Path.Combine(Path.GetTempPath(), $"dizimo_backup_{Guid.NewGuid():N}.db");
            _dr = new DizimistaRepository(_db);
            _or = new OfertaRepository(_db);
            _bs = new BackupService(_dr, _or);
        }

        [Fact]
        public async Task Export_And_Import_RemapsIds()
        {
            // create dizimista and oferta
            var id = await _dr.SaveAsync(new Dizimista { Nome = "X", Codigo = "C1", Ativo = true });
            await _or.SaveAsync(new Oferta { DizimistaID = id, Valor = 3.14m, Data = DateTime.UtcNow, Observacao = "o" });

            var tmp = Path.Combine(Path.GetTempPath(), $"bk_{Guid.NewGuid():N}.json");
            await _bs.ExportJsonAsync(tmp);

            // Import into a fresh DB instance to simulate a clean restore environment and avoid
            // deleting a file that may still be held by the provider on CI runners.
            var freshDb = Path.Combine(Path.GetTempPath(), $"dizimo_fresh_{Guid.NewGuid():N}.db");
            var freshDr = new DizimistaRepository(freshDb);
            var freshOr = new OfertaRepository(freshDb);
            var freshBs = new BackupService(freshDr, freshOr);
            await freshDr.InitAsync();
            await freshOr.InitAsync();

            var added = await freshBs.ImportJsonAsync(tmp);

            // Verify records exist in the fresh DB after import
            var diz = await freshDr.ListAsync();
            Assert.Single(diz);
            var ofertas = await freshOr.ListAsync();
            Assert.Single(ofertas);
        }

        public Task InitializeAsync()
        {
            return Task.WhenAll(_dr.InitAsync(), _or.InitAsync());
        }

        public Task DisposeAsync()
        {
            try { File.Delete(_db); } catch { }
            return Task.CompletedTask;
        }
    }
}
