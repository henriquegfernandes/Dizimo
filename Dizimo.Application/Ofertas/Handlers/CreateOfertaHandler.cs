using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using Dizimo.Domain.Entities;
using System.Threading.Tasks;

namespace Dizimo.Application.Ofertas.Handlers;

public class CreateOfertaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateOfertaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateOfertaCommand command)
    {
        var oferta = new Oferta
        {
            Id = Guid.NewGuid(),
            DizimistaId = command.DizimistaId,
            Valor = command.Valor,
            Data = command.Data
        };
        await _unitOfWork.Ofertas.AddAsync(oferta);
        await _unitOfWork.SaveChangesAsync();
        return oferta.Id;
    }
}
