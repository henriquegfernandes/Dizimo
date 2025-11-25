using System;

namespace Dizimo.Application.Dizimistas.Commands;

public record CreateDizimistaCommand(int NumeroCadastro, string Nome, DateTime DataNascimento);
public record UpdateDizimistaCommand(Guid Id, int NumeroCadastro, string Nome, DateTime DataNascimento, bool Ativo);
public record DeleteDizimistaCommand(Guid Id);
public record InativarDizimistaCommand(Guid Id);
