using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Globalization;
using System.Text;

namespace Dizimo.Infrastructure.Services;

public class OfertaCsvService
{
    private readonly IUnitOfWork _unitOfWork;
    public OfertaCsvService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<string> ExportarAsync()
    {
        var ofertas = await _unitOfWork.Ofertas.GetAllAsync();
        var sb = new StringBuilder();
        sb.AppendLine("DizimistaId,Valor,Data");
        foreach (var o in ofertas)
        {
            sb.AppendLine($"{o.DizimistaId},{o.Valor.ToString(CultureInfo.InvariantCulture)},{o.Data:yyyy-MM-dd}");
        }
        return sb.ToString();
    }

    public async Task<List<Oferta>> ImportarAsync(string csv)
    {
        var result = new List<Oferta>();
        var lines = csv.Split('\n');
        foreach (var line in lines.Skip(1)) // pula cabeçalho
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;
            if (!Guid.TryParse(parts[0], out var dizimistaId)) continue;
            if (!decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var valor)) continue;
            if (!DateTime.TryParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)) continue;
            result.Add(new Oferta {
                Id = Guid.NewGuid(),
                DizimistaId = dizimistaId,
                Valor = valor,
                Data = data
            });
        }
        return result;
    }
}
