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
    public class BackupServiceEdgeTests : IAsyncLifetime
    {
        private readonly string _db;
        private readonly DizimistaRepository _dr;
        private readonly OfertaRepository _or;
        private readonly BackupService _bs;

        public BackupServiceEdgeTests()
        {
            _db = Path.Combine(Path.GetTempPath(), $"dizimo_backup_{Guid.NewGuid():N}.db");
            _dr = new DizimistaRepository(_db);
            _or = new OfertaRepository(_db);
            _bs = new BackupService(_dr, _or);
        }

        public Task InitializeAsync() => Task.WhenAll(_dr.InitAsync(), _or.InitAsync());

        public Task DisposeAsync()
        {
            try { File.Delete(_db); } catch { }
            return Task.CompletedTask;
        }

        [Fact]
        public async Task OrphanOferta_IsSkipped()
        {
            // Create backup JSON with an oferta referencing a non-existent dizimista id
            var payload = new
            {
                Dizimistas = new object[] { },
                Ofertas = new[] {
                    new { ID = 1, DizimistaID = 9999, Valor = 1.23m, Data = DateTime.UtcNow.ToString("o"), Observacao = "x" }
                }
            };
            var tmp = Path.Combine(Path.GetTempPath(), $"bk_{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(payload));

            var added = await _bs.ImportJsonAsync(tmp);
            // no dizimistas created
            Assert.Equal(0, added);
            var ofertas = await _or.ListAsync();
            Assert.Empty(ofertas);
        }

        [Fact]
        public async Task MalformedDate_IsSkipped()
        {
            var payload = new
            {
                Dizimistas = new[] { new { ID = 1, Nome = "A", Codigo = "C1" } },
                Ofertas = new[] {
                    new { ID = 1, DizimistaID = 1, Valor = 2.0m, Data = "not-a-date", Observacao = "o" }
                }
            };
            var tmp = Path.Combine(Path.GetTempPath(), $"bk_{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(payload));

            var added = await _bs.ImportJsonAsync(tmp);
            // dizimista should be created, oferta skipped due to malformed date
            Assert.Equal(1, added);
            var diz = await _dr.ListAsync();
            Assert.Single(diz);
            var ofertas = await _or.ListAsync();
            Assert.Empty(ofertas);
        }
    }
}
