using Dizimo.Application.Ofertas.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Models;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Ofertas.Handlers;

public class GetOfertaHandlers
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOfertaHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Oferta?> Handle(GetOfertaByIdQuery query)
    {
        return await _unitOfWork.Ofertas.GetByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Oferta>> Handle(GetOfertasByDizimistaQuery query)
    {
        return await _unitOfWork.Ofertas.GetByDizimistaAsync(query.DizimistaId);
    }

    public async Task<IEnumerable<Oferta>> Handle(GetOfertasByDateQuery query)
    {
        return await _unitOfWork.Ofertas.GetByDateAsync(query.Date);
    }

    public async Task<IEnumerable<Oferta>> Handle(SearchOfertasQuery query)
    {
        return await _unitOfWork.Ofertas.SearchAsync(query.Date, query.DizimistaId);
    }

    public async Task<IEnumerable<Oferta>> Handle(GetAllOfertasQuery query)
    {
        return await _unitOfWork.Ofertas.GetAllAsync();
    }

    public async Task<PaginatedResult<Oferta>> Handle(GetAllOfertasPaginatedQuery query)
    {
        return await _unitOfWork.Ofertas.GetAllPaginatedAsync(
            query.PageNumber,
            query.PageSize,
            query.DataInicio,
            query.DataFim,
            query.TipoPagamento,
            query.FiltroNome);
    }
}