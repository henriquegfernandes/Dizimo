using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Models;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Dizimistas.Handlers;

public class GetDizimistaHandlers
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDizimistaHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Dizimista?> Handle(GetDizimistaByIdQuery query)
    {
        return await _unitOfWork.Dizimistas.GetByIdAsync(query.Id);
    }

    public async Task<Dizimista?> Handle(GetDizimistaByNumeroCadastroQuery query)
    {
        return await _unitOfWork.Dizimistas.GetByNumeroCadastroAsync(query.NumeroCadastro);
    }

    public async Task<IEnumerable<Dizimista>> Handle(GetAllDizimistasQuery query)
    {
        return await _unitOfWork.Dizimistas.GetAllAsync();
    }

    public async Task<PaginatedResult<Dizimista>> Handle(GetAllDizimistasPaginatedQuery query)
    {
        return await _unitOfWork.Dizimistas.GetAllPaginatedAsync(
            query.PageNumber,
            query.PageSize,
            query.FiltroNome,
            query.StatusSelecionado);
    }

    public async Task<IEnumerable<Dizimista>> Handle(GetAniversariantesQuery query)
    {
        return await _unitOfWork.Dizimistas.GetAniversariantesAsync(query.Mes);
    }
}