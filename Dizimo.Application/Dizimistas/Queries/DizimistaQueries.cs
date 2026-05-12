namespace Dizimo.Application.Dizimistas.Queries;

public record GetDizimistaByIdQuery(Guid Id);

public record GetDizimistaByNumeroCadastroQuery(int NumeroCadastro);

public record GetAllDizimistasQuery;

public record GetAllDizimistasPaginatedQuery(
    int PageNumber,
    int PageSize,
    string? FiltroNome = null,
    string? StatusSelecionado = null);

public record GetAniversariantesQuery(int Mes);