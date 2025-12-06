using Dizimo.Domain.Repositories;
using Dizimo.Application.Usuarios.Commands;
using System.Threading.Tasks;

namespace Dizimo.Application.Usuarios.Handlers;

public class InativarUsuarioHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public InativarUsuarioHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(InativarUsuarioCommand command)
    {
        await _unitOfWork.Usuarios.InativarAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}
