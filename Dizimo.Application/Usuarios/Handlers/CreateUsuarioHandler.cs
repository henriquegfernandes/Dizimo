using System;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Domain.Entities;
using System.Threading.Tasks;

namespace Dizimo.Application.Usuarios.Handlers;

public class CreateUsuarioHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateUsuarioHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateUsuarioCommand command)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = command.Nome,
            Login = command.Login,
            SenhaHash = command.Senha, // Hash deve ser aplicado na infraestrutura
            Perfil = PerfilUsuario.Padrao,
            Ativo = true
        };
        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();
        return usuario.Id;
    }
}
