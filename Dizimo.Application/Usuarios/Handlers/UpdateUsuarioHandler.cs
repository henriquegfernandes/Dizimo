using Dizimo.Domain.Repositories;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Domain.Entities;
using System.Threading.Tasks;

namespace Dizimo.Application.Usuarios.Handlers;

public class UpdateUsuarioHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateUsuarioHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateUsuarioCommand command)
    {
        var usuario = new Usuario
        {
            Id = command.Id,
            Nome = command.Nome,
            Login = command.Login,
            SenhaHash = command.Senha, // Hash deve ser aplicado na infraestrutura
            Ativo = command.Ativo
        };
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();
    }
}
