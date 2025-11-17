using System;
using System.IO;
using System.Threading.Tasks;
using Dizimo.Core.Data;
using Dizimo.Core.Models;
using Xunit;

namespace Dizimo.Tests
{
    public class OfertaRepositoryTests : IAsyncLifetime
    {
        private readonly string _db;
        private readonly OfertaRepository _repo;

        public OfertaRepositoryTests()
        {
            _db = Path.Combine(Path.GetTempPath(), $"dizimo_offerta_{Guid.NewGuid():N}.db");
            _repo = new OfertaRepository(_db);
        }

        [Fact]
        public async Task Save_And_List_Work()
        {
            var o = new Oferta { DizimistaID = 1, Valor = 12.34m, Data = DateTime.UtcNow, Observacao = "test" };
            var id = await _repo.SaveAsync(o);
            Assert.True(id > 0);

            var list = await _repo.ListAsync();
            Assert.Contains(list, x => x.ID == id && x.Valor == o.Valor && x.Observacao == o.Observacao);
        }

        [Fact]
        public async Task ListByDate_Filters()
        {
            var today = DateTime.UtcNow.Date;
            await _repo.SaveAsync(new Oferta { DizimistaID = 1, Valor = 1m, Data = today.AddHours(2), Observacao = "a" });
            await _repo.SaveAsync(new Oferta { DizimistaID = 1, Valor = 2m, Data = today.AddDays(-1), Observacao = "b" });

            var list = await _repo.ListByDateAsync(today);
            Assert.Single(list);
            Assert.Equal(1m, list[0].Valor);
        }

        public Task InitializeAsync()
        {
            return _repo.InitAsync();
        }

        public Task DisposeAsync()
        {
            try { File.Delete(_db); } catch { }
            return Task.CompletedTask;
        }
    }
}
