using Dizimo.Application.Usuarios.Commands;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Usuarios.Handlers;

public class DeleteUsuarioHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUsuarioHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUsuarioCommand command)
    {
        await _unitOfWork.Usuarios.DeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}