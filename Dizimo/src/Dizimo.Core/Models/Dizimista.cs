using System;
namespace Dizimo.Core.Models;

public class Dizimista
{
    public int ID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Bloqueado { get; set; } = false;
}
