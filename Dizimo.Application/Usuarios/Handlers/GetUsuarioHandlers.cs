using Dizimo.Application.Usuarios.Queries;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Usuarios.Handlers;

public class GetUsuarioHandlers
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsuarioHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Usuario?> Handle(GetUsuarioByIdQuery query)
    {
        return await _unitOfWork.Usuarios.GetByIdAsync(query.Id);
    }

    public async Task<Usuario?> Handle(GetUsuarioByLoginQuery query)
    {
        return await _unitOfWork.Usuarios.GetByLoginAsync(query.Login);
    }

    public async Task<IEnumerable<Usuario>> Handle(GetAllUsuariosQuery query)
    {
        return await _unitOfWork.Usuarios.GetAllAsync();
    }
}