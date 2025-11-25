using System;

namespace Dizimo.Application.Ofertas.Commands;

public record CreateOfertaCommand(Guid DizimistaId, decimal Valor, DateTime Data);
public record UpdateOfertaCommand(Guid Id, Guid DizimistaId, decimal Valor, DateTime Data);
public record DeleteOfertaCommand(Guid Id);
