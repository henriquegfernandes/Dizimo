using System;

namespace Dizimo.Domain.Entities;

public class OfertaComDizimista
{
    public Guid Id { get; set; }
    public Guid DizimistaId { get; set; }
    public string NomeDizimista { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public int MesReferencia { get; set; }
    public int AnoReferencia { get; set; }
}