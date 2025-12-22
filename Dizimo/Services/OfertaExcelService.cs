using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using ClosedXML.Excel;
using System.Globalization;

namespace Dizimo.Services;

public class OfertaExcelService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OfertaExcelService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ExportarAsync(List<Oferta>? ofertas = null)
    {
        // Se nenhuma lista for passada, exporta todas
        if (ofertas == null)
        {
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            ofertas = todasOfertas is List<Oferta> l ? l : todasOfertas.ToList();
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ofertas");

        // Cabeçalho
        var headerRow = worksheet.Row(1);
        headerRow.Cell(1).Value = "Código Dizimista";
        headerRow.Cell(2).Value = "Nome Dizimista";
        headerRow.Cell(3).Value = "Valor";
        headerRow.Cell(4).Value = "Data";
        headerRow.Cell(5).Value = "Mês Referência";
        headerRow.Cell(6).Value = "Ano Referência";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:F1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dados
        decimal totalValor = 0;
        int rowNumber = 2;
        
        foreach (var o in ofertas)
        {
            var dizimista = await _unitOfWork.Dizimistas.GetByIdAsync(o.DizimistaId);
            var codigoDizimista = dizimista?.NumeroCadastro.ToString() ?? "";
            var nomeDizimista = dizimista?.Nome ?? "";

            worksheet.Cell(rowNumber, 1).Value = codigoDizimista;
            worksheet.Cell(rowNumber, 2).Value = nomeDizimista;
            worksheet.Cell(rowNumber, 3).Value = o.Valor;
            worksheet.Cell(rowNumber, 4).Value = o.Data;
            worksheet.Cell(rowNumber, 5).Value = o.MesReferencia;
            worksheet.Cell(rowNumber, 6).Value = o.AnoReferencia;

            totalValor += o.Valor;
            rowNumber++;
        }

        // Linha de total
        var totalRow = rowNumber + 1;
        worksheet.Cell(totalRow, 1).Value = "TOTAL";
        worksheet.Cell(totalRow, 3).Value = totalValor;
        worksheet.Cell(totalRow, 3).Style.Font.Bold = true;
        worksheet.Cell(totalRow, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;

        // Ajustar largura das colunas
        worksheet.Column(1).Width = 18;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 15;
        worksheet.Column(3).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Column(4).Width = 15;
        worksheet.Column(4).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 15;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public MemoryStream GerarModelo()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ofertas");

        // Cabeçalho
        var headerRow = worksheet.Row(1);
        headerRow.Cell(1).Value = "Código Dizimista";
        headerRow.Cell(2).Value = "Valor";
        headerRow.Cell(3).Value = "Data";
        headerRow.Cell(4).Value = "Mês Referência";
        headerRow.Cell(5).Value = "Ano Referência";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:E1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Exemplo de linha
        worksheet.Cell(2, 1).Value = 123;
        worksheet.Cell(2, 2).Value = 100.00;
        worksheet.Cell(2, 3).Value = DateTime.Now;
        worksheet.Cell(2, 4).Value = 1;
        worksheet.Cell(2, 5).Value = DateTime.Now.Year;

        // Linhas em branco para o usuário preencher (5 linhas)
        for (int i = 3; i <= 7; i++)
        {
            for (int j = 1; j <= 5; j++)
            {
                worksheet.Cell(i, j).Value = "";
            }
        }

        // Ajustar largura das colunas
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 15;
        worksheet.Column(2).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Column(3).Width = 15;
        worksheet.Column(3).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(4).Width = 18;
        worksheet.Column(5).Width = 18;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public class ResultadoImportacao
    {
        public List<Oferta> OfertasImportadas { get; set; } = new();
        public List<string> Erros { get; set; } = new();
    }

    public async Task<ResultadoImportacao> ImportarAsync(byte[] excelBytes)
    {
        var resultado = new ResultadoImportacao();

        using var stream = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();

        int numeroLinha = 2; // Começa em 2 (linha 1 é cabeçalho)
        int lastRow = 1;
        var lastRowUsed = worksheet.LastRowUsed();
        if (lastRowUsed != null)
        {
            lastRow = lastRowUsed.RowNumber();
        }

        for (int rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var codigoDizimistaCell = worksheet.Cell(rowNumber, 1).GetValue<string>();
            if (string.IsNullOrWhiteSpace(codigoDizimistaCell)) continue;

            if (!int.TryParse(codigoDizimistaCell.Trim(), out var codigoDizimista))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Código do dizimista '{codigoDizimistaCell.Trim()}' é inválido (deve ser um número)");
                numeroLinha++;
                continue;
            }

            var dizimista = dizimistas.FirstOrDefault(d => d.NumeroCadastro == codigoDizimista);
            if (dizimista == null)
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Dizimista com código {codigoDizimista} não encontrado no sistema");
                numeroLinha++;
                continue;
            }

            var valorCell = worksheet.Cell(rowNumber, 2).GetValue<string>();
            if (!decimal.TryParse(valorCell.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Valor '{valorCell.Trim()}' é inválido");
                numeroLinha++;
                continue;
            }

            var dataCell = worksheet.Cell(rowNumber, 3).GetValue<string>();
            if (!DateTime.TryParseExact(dataCell.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                if (!DateTime.TryParse(dataCell.Trim(), out data))
                {
                    resultado.Erros.Add($"Linha {numeroLinha}: Data '{dataCell.Trim()}' está em formato inválido (use dd/mm/yyyy)");
                    numeroLinha++;
                    continue;
                }
            }

            var mesReferenciaCell = worksheet.Cell(rowNumber, 4).GetValue<string>();
            if (!int.TryParse(mesReferenciaCell.Trim(), out var mesReferencia) || mesReferencia < 1 || mesReferencia > 12)
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Mês de referência '{mesReferenciaCell.Trim()}' é inválido (deve ser entre 1 e 12)");
                numeroLinha++;
                continue;
            }

            var anoReferenciaCell = worksheet.Cell(rowNumber, 5).GetValue<string>();
            if (!int.TryParse(anoReferenciaCell.Trim(), out var anoReferencia))
            {
                resultado.Erros.Add($"Linha {numeroLinha}: Ano de referência '{anoReferenciaCell.Trim()}' é inválido");
                numeroLinha++;
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

            numeroLinha++;
        }

        return resultado;
    }
}
