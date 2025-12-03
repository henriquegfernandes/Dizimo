using System;

namespace Dizimo.Domain.Entities;

public class Endereco
{
    public string Rua { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = "Osasco";
    public string UF { get; set; } = "SP";
    public string CEP { get; set; } = string.Empty;
}

public class Dizimista
{
    public Guid Id { get; set; }
    public int NumeroCadastro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public Endereco Endereco { get; set; } = new Endereco();
    public string Telefone { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.Today;
}
