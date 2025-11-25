namespace Dizimo.Application.Dizimistas.Queries;

public record GetDizimistaByIdQuery(Guid Id);
public record GetDizimistaByNumeroCadastroQuery(int NumeroCadastro);
public record GetAllDizimistasQuery();
public record GetAniversariantesQuery(int Mes);
