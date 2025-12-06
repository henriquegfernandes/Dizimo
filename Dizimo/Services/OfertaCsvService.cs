using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dizimo.Services;

public class OfertaCsvService
{
    private readonly IUnitOfWork _unitOfWork;
    public OfertaCsvService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<string> ExportarAsync(List<Oferta>? ofertas = null)
    {
        // Se nenhuma lista for passada, exporta todas
        if (ofertas == null)
        {
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            ofertas = todasOfertas is List<Oferta> l ? l : todasOfertas.ToList();
        }

        var sb = new StringBuilder();
        sb.AppendLine("CodigoDizimista,NomeDizimista,Valor,Data,MesReferencia,AnoReferencia");
        
        decimal totalValor = 0;
        foreach (var o in ofertas)
        {
            // Buscar dados do dizimista
            var dizimista = await _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId);
            var codigoDizimista = dizimista?.NumeroCadastro.ToString() ?? "";
            var nomeDizimista = dizimista?.Nome ?? "";
            
            sb.AppendLine($"{codigoDizimista},\"{nomeDizimista}\",{o.Valor.ToString(CultureInfo.InvariantCulture)},{o.Data:yyyy-MM-dd},{o.MesReferencia},{o.AnoReferencia}");
            totalValor += o.Valor;
        }
        
        // Adicionar linha de total
        sb.AppendLine();
        sb.AppendLine($"TOTAL,{totalValor.ToString(CultureInfo.InvariantCulture)}");
        
        return sb.ToString();
    }

    public string GerarModelo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("CodigoDizimista,Valor,Data,MesReferencia,AnoReferencia");
        // Exemplo de linha de modelo
        sb.AppendLine("123,100.00,2024-01-01,1,2024");
        return sb.ToString();
    }

    public class ResultadoImportacao
    {
        public List<Oferta> OfertasImportadas { get; set; } = new();
        public List<string> Erros { get; set; } = new();
    }

    public async Task<ResultadoImportacao> ImportarAsync(string csv)
    {
        var resultado = new ResultadoImportacao();
        var linhas = csv.Split('\n');
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        
        int numeroLinha = 1; // Começa em 1 (cabeçalho é linha 1)
        
        foreach (var linha in linhas.Skip(1)) // pula cabeçalho
        {
            numeroLinha++;
            
            if (string.IsNullOrWhiteSpace(linha)) continue;
            
            var partes = linha.Split(',');
            if (partes.Length < 5)
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Formato inválido. Esperado: CodigoDizimista,Valor,Data,MesReferencia,AnoReferencia");
                continue;
            }
            
            // Buscar dizimista pelo código
            if (!int.TryParse(partes[0].Trim(), out var codigoDizimista))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Código do dizimista '{partes[0].Trim()}' é inválido (deve ser um número)");
                continue;
            }
            
            var dizimista = dizimistas.FirstOrDefault(d => d.NumeroCadastro == codigoDizimista);
            if (dizimista == null)
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Dizimista com código {codigoDizimista} não encontrado no sistema");
                continue;
            }
            
            if (!decimal.TryParse(partes[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Valor '{partes[1].Trim()}' é inválido");
                continue;
            }
            
            if (!DateTime.TryParseExact(partes[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Data '{partes[2].Trim()}' está em formato inválido (use yyyy-MM-dd)");
                continue;
            }
            
            if (!int.TryParse(partes[3].Trim(), out var mesReferencia) || mesReferencia < 1 || mesReferencia > 12)
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Mês de referência '{partes[3].Trim()}' é inválido (deve ser entre 1 e 12)");
                continue;
            }
            
            if (!int.TryParse(partes[4].Trim(), out var anoReferencia))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Ano de referência '{partes[4].Trim()}' é inválido");
                continue;
            }
            
            resultado.OfertasImportadas.Add(new Oferta
            {
                Id = Guid.NewGuid(),
                DizimistaId = dizimista.Id,
                Valor = valor,
                Data = data,
                MesReferencia = mesReferencia,
                AnoReferencia = anoReferencia
            });
        }
        
        return resultado;
    }
}
