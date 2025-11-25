using Dizimo.Domain.Repositories;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dizimo.Application.Dizimistas.Handlers;

public class GetDizimistaHandlers
{
    private readonly IUnitOfWork _unitOfWork;
    public GetDizimistaHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Dizimista?> Handle(GetDizimistaByIdQuery query) => await _unitOfWork.Dizimistas.GetByIdAsync(query.Id);
    public async Task<Dizimista?> Handle(GetDizimistaByNumeroCadastroQuery query) => await _unitOfWork.Dizimistas.GetByNumeroCadastroAsync(query.NumeroCadastro);
    public async Task<IEnumerable<Dizimista>> Handle(GetAllDizimistasQuery query) => await _unitOfWork.Dizimistas.GetAllAsync();
    public async Task<IEnumerable<Dizimista>> Handle(GetAniversariantesQuery query) => await _unitOfWork.Dizimistas.GetAniversariantesAsync(query.Mes);
}
