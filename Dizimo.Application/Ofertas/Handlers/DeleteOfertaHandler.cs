using Dizimo.Domain.Repositories;
using Dizimo.Application.Ofertas.Commands;
using System.Threading.Tasks;

namespace Dizimo.Application.Ofertas.Handlers;

public class DeleteOfertaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteOfertaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteOfertaCommand command)
    {
        await _unitOfWork.Ofertas.DeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}
