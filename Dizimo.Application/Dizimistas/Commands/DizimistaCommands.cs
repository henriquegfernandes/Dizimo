using Dizimo.Domain.Entities;
using System;

namespace Dizimo.Application.Dizimistas.Commands;

public record CreateDizimistaCommand(
    int NumeroCadastro,
    string Nome,
    DateTime DataNascimento,
    Endereco Endereco,
    string Telefone,
    string Whatsapp,
    DateTime DataCadastro
);

public record UpdateDizimistaCommand(
    Guid Id,
    int NumeroCadastro,
    string Nome,
    DateTime DataNascimento,
    bool Ativo,
    Endereco Endereco,
    string Telefone,
    string Whatsapp,
    DateTime DataCadastro
);

public record DeleteDizimistaCommand(Guid Id);
public record InativarDizimistaCommand(Guid Id);
