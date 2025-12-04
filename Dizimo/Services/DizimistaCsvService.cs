using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Globalization;
using System.Text;

namespace Dizimo.Services;

public class DizimistaCsvService
{
    private readonly IUnitOfWork _unitOfWork;
    public DizimistaCsvService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<string> ExportarAsync()
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var sb = new StringBuilder();
        
        // Cabeçalho com todos os campos
        sb.AppendLine("NumeroCadastro,Nome,DataNascimento,Telefone,Whatsapp,DataCadastro,Ativo," +
                      "Rua,Numero,Complemento,Bairro,Cidade,UF,CEP");
        
        foreach (var d in dizimistas)
        {
            var endereco = d.Endereco ?? new Endereco();
            var linha = $"{d.NumeroCadastro}," +
                       $"\"{d.Nome}\"," +
                       $"{d.DataNascimento:yyyy-MM-dd}," +
                       $"\"{d.Telefone}\"," +
                       $"\"{d.Whatsapp}\"," +
                       $"{d.DataCadastro:yyyy-MM-dd}," +
                       $"{d.Ativo}," +
                       $"\"{endereco.Rua}\"," +
                       $"\"{endereco.Numero}\"," +
                       $"\"{endereco.Complemento}\"," +
                       $"\"{endereco.Bairro}\"," +
                       $"\"{endereco.Cidade}\"," +
                       $"\"{endereco.UF}\"," +
                       $"\"{endereco.CEP}\"";
            sb.AppendLine(linha);
        }
        return sb.ToString();
    }

    public async Task<List<Dizimista>> ImportarAsync(string csv)
    {
        var result = new List<Dizimista>();
        var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var parts = ParseCsvLine(line);
            if (parts.Length < 14) continue; // Deve ter pelo menos 14 campos
            
            // Parse dos campos obrigatórios
            if (!int.TryParse(parts[0].Trim(), out var numeroCadastro)) continue;
            var nome = parts[1].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(nome)) continue;
            
            if (!DateTime.TryParseExact(parts[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNascimento))
                continue;
            
            // Parse dos campos opcionais
            var telefone = parts[3].Trim().Trim('"');
            var whatsapp = parts[4].Trim().Trim('"');
            
            var dataCadastro = DateTime.Today;
            if (DateTime.TryParseExact(parts[5].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dc))
                dataCadastro = dc;
            
            if (!bool.TryParse(parts[6].Trim(), out var ativo))
                ativo = true;
            
            // Endereço
            var endereco = new Endereco
            {
                Rua = parts[7].Trim().Trim('"'),
                Numero = parts[8].Trim().Trim('"'),
                Complemento = parts[9].Trim().Trim('"'),
                Bairro = parts[10].Trim().Trim('"'),
                Cidade = parts[11].Trim().Trim('"'),
                UF = parts[12].Trim().Trim('"'),
                CEP = parts[13].Trim().Trim('"')
            };
            
            result.Add(new Dizimista
            {
                Id = Guid.NewGuid(),
                NumeroCadastro = numeroCadastro,
                Nome = nome,
                DataNascimento = dataNascimento,
                Telefone = telefone,
                Whatsapp = whatsapp,
                DataCadastro = dataCadastro,
                Ativo = ativo,
                Endereco = endereco
            });
        }
        return result;
    }

    /// <summary>
    /// Parse CSV respeitando aspas para valores que contêm vírgulas
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        
        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    /// <summary>
    /// Gera a planilha modelo com dados de exemplo
    /// </summary>
    public string GerarModeloAsync()
    {
        var sb = new StringBuilder();
        
        // Cabeçalho
        sb.AppendLine("NumeroCadastro,Nome,DataNascimento,Telefone,Whatsapp,DataCadastro,Ativo," +
                      "Rua,Numero,Complemento,Bairro,Cidade,UF,CEP");
        
        // Exemplo de linha preenchida
        sb.AppendLine("1," +
                     "\"João da Silva\"," +
                     "1990-05-15," +
                     "\"11987654321\"," +
                     "\"11987654321\"," +
                     "2024-01-15," +
                     "True," +
                     "\"Rua das Flores\"," +
                     "\"123\"," +
                     "\"Apto 42\"," +
                     "\"Centro\"," +
                     "\"São Paulo\"," +
                     "\"SP\"," +
                     "\"01310-100\"");
        
        // Exemplos de linhas em branco para o usuário preencher
        for (int i = 0; i < 5; i++)
        {
            sb.AppendLine("," +
                         "\"\"," +
                         "," +
                         "\"\"," +
                         "\"\"," +
                         "," +
                         "," +
                         "\"\"," +
                         "\"\"," +
                         "\"\"," +
                         "\"\"," +
                         "\"\"," +
                         "\"\"," +
                         "\"\"");
        }
        
        return sb.ToString();
    }
}
