using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dizimo.Core.Data;
using Dizimo.Core.Models;
using Dizimo.Core.Services;
using Xunit;

namespace Dizimo.Tests
{
    public class DizimoServiceTests : IAsyncLifetime
    {
        private readonly string _db1;
        private readonly DizimistaRepository _dr;
        private readonly OfertaRepository _or;
        private readonly DizimoService _service;

        public DizimoServiceTests()
        {
            _db1 = Path.Combine(Path.GetTempPath(), $"dizimista_{Guid.NewGuid():N}.db");
            _dr = new DizimistaRepository(_db1);
            _or = new OfertaRepository(_db1);
            _service = new DizimoService(_dr, _or);
        }

        [Fact]
        public async Task RelatorioGeral_AggregatesTotals()
        {
            var d = new Dizimista { Nome = "A", Codigo = "001", Ativo = true };
            var id = await _dr.SaveAsync(d);
            await _or.SaveAsync(new Oferta { DizimistaID = id, Valor = 5.5m, Data = DateTime.UtcNow, Observacao = "x" });
            await _or.SaveAsync(new Oferta { DizimistaID = id, Valor = 4.5m, Data = DateTime.UtcNow, Observacao = "y" });

            var report = await _service.RelatorioGeralAsync();
            var row = report.First(r => r.Dizimista.ID == id);
            Assert.Equal(2, row.Count);
            Assert.Equal(10m, row.Total);
        }

        public Task InitializeAsync()
        {
            return Task.WhenAll(_dr.InitAsync(), _or.InitAsync());
        }

        public Task DisposeAsync()
        {
            try { File.Delete(_db1); } catch { }
            return Task.CompletedTask;
        }
    }
}
