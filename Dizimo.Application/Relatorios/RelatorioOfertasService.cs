using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Dizimo.Application.Relatorios;

public class RelatorioOfertasService
{
    private readonly IUnitOfWork _unitOfWork;
    public RelatorioOfertasService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<Oferta>> GetOfertasPorDataAsync(DateTime data)
    {
        var ofertas = await _unitOfWork.Ofertas.GetByDateAsync(data);
        return ofertas;
    }

    public async Task<IEnumerable<Oferta>> GetOfertasPorDizimistaAsync(Guid dizimistaId)
    {
        var ofertas = await _unitOfWork.Ofertas.GetByDizimistaAsync(dizimistaId);
        return ofertas;
    }

    public async Task<decimal> GetTotalOfertasPorDataAsync(DateTime data)
    {
        var ofertas = await _unitOfWork.Ofertas.GetByDateAsync(data);
        return ofertas.Sum(o => o.Valor);
    }
}
