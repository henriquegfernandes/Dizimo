namespace Dizimo.Domain.Entities;

public class Oferta
{
    public Guid Id { get; set; }
    public Guid DizimistaId { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
}
