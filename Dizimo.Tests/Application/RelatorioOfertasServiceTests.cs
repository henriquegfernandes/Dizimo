using Dizimo.Application.Relatorios;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dizimo.Tests.Application;

public class RelatorioOfertasServiceTests
{
    [Fact]
    public async Task GetTotalOfertasPorDataAsync_ReturnsSum()
    {
        var ofertas = new List<Oferta>
        {
            new Oferta { Valor = 10, Data = new DateTime(2024, 6, 1) },
            new Oferta { Valor = 20, Data = new DateTime(2024, 6, 1) }
        };
        var repoMock = new Mock<IUnitOfWork>();
        repoMock.Setup(u => u.Ofertas.GetByDateAsync(new DateTime(2024, 6, 1))).ReturnsAsync(ofertas);
        var service = new RelatorioOfertasService(repoMock.Object);
        var total = await service.GetTotalOfertasPorDataAsync(new DateTime(2024, 6, 1));
        Assert.Equal(30, total);
    }
}
