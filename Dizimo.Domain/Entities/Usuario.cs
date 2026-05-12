using System;

namespace Dizimo.Domain.Entities;

public enum PerfilUsuario
{
    Padrao,
    Admin
}

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Padrao;
    public bool Ativo { get; set; } = true;
}