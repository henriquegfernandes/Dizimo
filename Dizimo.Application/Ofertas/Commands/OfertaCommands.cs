using System;

namespace Dizimo.Application.Ofertas.Commands;

public record CreateOfertaCommand(Guid DizimistaId, decimal Valor, DateTime Data, int MesReferencia, int AnoReferencia);
public record UpdateOfertaCommand(Guid Id, Guid DizimistaId, decimal Valor, DateTime Data, int MesReferencia, int AnoReferencia);
public record DeleteOfertaCommand(Guid Id);
