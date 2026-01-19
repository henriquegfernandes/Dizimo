using System;

namespace Dizimo.Application.Ofertas.Queries;

public record GetOfertaByIdQuery(Guid Id);
public record GetOfertasByDizimistaQuery(Guid DizimistaId);
public record GetOfertasByDateQuery(DateTime Date);
public record SearchOfertasQuery(DateTime? Date, Guid? DizimistaId);
public record GetAllOfertasQuery();
public record GetAllOfertasPaginatedQuery(
    int PageNumber, 
    int PageSize,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    string? TipoPagamento = null,
    string? FiltroNome = null);
