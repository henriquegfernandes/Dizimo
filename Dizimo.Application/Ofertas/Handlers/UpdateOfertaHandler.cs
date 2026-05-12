using Dizimo.Application.Ofertas.Commands;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Ofertas.Handlers;

public class UpdateOfertaHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOfertaHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateOfertaCommand command)
    {
        var oferta = new Oferta
        {
            Id = command.Id,
            DizimistaId = command.DizimistaId,
            Valor = command.Valor,
            Data = command.Data,
            MesReferencia = command.MesReferencia,
            AnoReferencia = command.AnoReferencia
        };
        await _unitOfWork.Ofertas.UpdateAsync(oferta);
        await _unitOfWork.SaveChangesAsync();
    }
}