namespace Dizimo.Application.Ofertas.Queries;

public record GetOfertaByIdQuery(Guid Id);
public record GetOfertasByDizimistaQuery(Guid DizimistaId);
public record GetOfertasByDateQuery(DateTime Date);
public record SearchOfertasQuery(DateTime? Date, Guid? DizimistaId);
public record GetAllOfertasQuery();
