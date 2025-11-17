using System;
namespace Dizimo.Models;

public class Oferta
{
    public int ID { get; set; }
    public int DizimistaID { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Observacao { get; set; } = string.Empty;
}
