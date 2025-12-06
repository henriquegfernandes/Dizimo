using System;

namespace Dizimo.Application.Usuarios.Queries;

public record GetUsuarioByIdQuery(Guid Id);
public record GetUsuarioByLoginQuery(string Login);
public record GetAllUsuariosQuery();
