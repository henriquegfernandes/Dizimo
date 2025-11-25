using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Globalization;
using System.Text;

namespace Dizimo.Infrastructure.Services;

public class DizimistaCsvService
{
    private readonly IUnitOfWork _unitOfWork;
    public DizimistaCsvService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<string> ExportarAsync()
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var sb = new StringBuilder();
        sb.AppendLine("NumeroCadastro,Nome,DataNascimento,Ativo");
        foreach (var d in dizimistas)
        {
            sb.AppendLine($"{d.NumeroCadastro},\"{d.Nome}\",{d.DataNascimento:yyyy-MM-dd},{d.Ativo}");
        }
        return sb.ToString();
    }

    public async Task<List<Dizimista>> ImportarAsync(string csv)
    {
        var result = new List<Dizimista>();
        var lines = csv.Split('\n');
        foreach (var line in lines.Skip(1)) // pula cabeçalho
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            if (!int.TryParse(parts[0], out var numeroCadastro)) continue;
            var nome = parts[1].Trim('"');
            if (!DateTime.TryParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNascimento)) continue;
            if (!bool.TryParse(parts[3], out var ativo)) ativo = true;
            result.Add(new Dizimista {
                Id = Guid.NewGuid(),
                NumeroCadastro = numeroCadastro,
                Nome = nome,
                DataNascimento = dataNascimento,
                Ativo = ativo
            });
        }
        return result;
    }
}
