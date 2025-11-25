using System;

namespace Dizimo.Domain.Entities;

public class Dizimista
{
    public Guid Id { get; set; }
    public int NumeroCadastro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
}
