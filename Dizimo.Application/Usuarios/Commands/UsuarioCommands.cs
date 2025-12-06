using System;
using Dizimo.Domain.Entities;

namespace Dizimo.Application.Usuarios.Commands;

public record CreateUsuarioCommand(string Nome, string Login, string Senha);
public record UpdateUsuarioCommand(Guid Id, string Nome, string Login, string Senha, bool Ativo, PerfilUsuario Perfil = PerfilUsuario.Padrao);
public record DeleteUsuarioCommand(Guid Id);
public record InativarUsuarioCommand(Guid Id);
