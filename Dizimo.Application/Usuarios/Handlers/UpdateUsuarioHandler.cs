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
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(command.Id);
        
        if (usuario != null)
        {
            usuario.Nome = command.Nome;
            usuario.Login = command.Login;
            
            // Atualizar senha apenas se não for vazia
            if (!string.IsNullOrWhiteSpace(command.Senha))
            {
                usuario.SenhaHash = command.Senha;
            }
            
            usuario.Ativo = command.Ativo;
            usuario.Perfil = command.Perfil;
            
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
