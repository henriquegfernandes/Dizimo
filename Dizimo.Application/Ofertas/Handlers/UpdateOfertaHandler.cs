using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;

namespace Dizimo.Application.Ofertas.Handlers;

public class UpdateOfertaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateOfertaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateOfertaCommand command)
    {
        var oferta = new Oferta
        {
            Id = command.Id,
            DizimistaId = command.DizimistaId,
            Valor = command.Valor,
            Data = command.Data
        };
        await _unitOfWork.Ofertas.UpdateAsync(oferta);
        await _unitOfWork.SaveChangesAsync();
    }
}
