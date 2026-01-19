using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Queries;
using Dizimo.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dizimo.Domain.Models;

namespace Dizimo.Application.Ofertas.Handlers;

public class GetOfertaHandlers
{
    private readonly IUnitOfWork _unitOfWork;
    public GetOfertaHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Oferta?> Handle(GetOfertaByIdQuery query) => await _unitOfWork.Ofertas.GetByIdAsync(query.Id);
    public async Task<IEnumerable<Oferta>> Handle(GetOfertasByDizimistaQuery query) => await _unitOfWork.Ofertas.GetByDizimistaAsync(query.DizimistaId);
    public async Task<IEnumerable<Oferta>> Handle(GetOfertasByDateQuery query) => await _unitOfWork.Ofertas.GetByDateAsync(query.Date);
    public async Task<IEnumerable<Oferta>> Handle(SearchOfertasQuery query) => await _unitOfWork.Ofertas.SearchAsync(query.Date, query.DizimistaId);
    public async Task<IEnumerable<Oferta>> Handle(GetAllOfertasQuery query) => await _unitOfWork.Ofertas.GetAllAsync();
    public async Task<PaginatedResult<Oferta>> Handle(GetAllOfertasPaginatedQuery query) 
        => await _unitOfWork.Ofertas.GetAllPaginatedAsync(
            query.PageNumber, 
            query.PageSize,
            query.DataInicio,
            query.DataFim,
            query.TipoPagamento);
}
