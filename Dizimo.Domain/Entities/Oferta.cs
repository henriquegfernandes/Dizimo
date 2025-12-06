using System;

namespace Dizimo.Domain.Entities;

public class Oferta
{
    public Guid Id { get; set; }
    public Guid DizimistaId { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
}
