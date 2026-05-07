using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using ClosedXML.Excel;
using System.Globalization;

namespace Dizimo.Application.Reporting.Services;

public class OfertaExcelService(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ExportarAsync(List<Oferta>? ofertas = null, DateTime? dataInicio = null, DateTime? dataFim = null, string? tipoPagamento = null, string? filtroNome = null)
    {
        // Se nenhuma lista for passada, busca todas as ofertas
        if (ofertas == null)
        {
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            ofertas = todasOfertas is List<Oferta> l ? l : [.. todasOfertas];
        }

        // Buscar todos os dizimistas uma única vez
        var todosDizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var dicionarioDizimistas = todosDizimistas.ToDictionary(d => d.Id);

        // Aplicar filtros
        var ofertasFiltradas = ofertas.AsEnumerable();

        // Filtro de data início
        if (dataInicio.HasValue)
            ofertasFiltradas = ofertasFiltradas.Where(o => o.Data.Date >= dataInicio.Value.Date);

        // Filtro de data fim
        if (dataFim.HasValue)
            ofertasFiltradas = ofertasFiltradas.Where(o => o.Data.Date <= dataFim.Value.Date);

        // Filtro de tipo de pagamento
        if (!string.IsNullOrWhiteSpace(tipoPagamento) && tipoPagamento != "Todos")
        {
            if (Enum.TryParse<TipoPagamento>(tipoPagamento, out var tipo))
                ofertasFiltradas = ofertasFiltradas.Where(o => o.TipoPagamento == tipo);
        }

        // Filtro de nome do dizimista (usando dicionário em memória)
        if (!string.IsNullOrWhiteSpace(filtroNome))
        {
            ofertasFiltradas = ofertasFiltradas.Where(oferta =>
            {
                if (dicionarioDizimistas.TryGetValue(oferta.DizimistaId, out var dizimista))
                {
                    return dizimista.Nome.Contains(filtroNome, StringComparison.OrdinalIgnoreCase) ||
                           dizimista.NumeroCadastro.ToString().Contains(filtroNome);
                }
                return false;
            });
        }

        // Aplicar a mesma ordenação que a página: OrderByDescending(o => o.Data).ThenBy(o => o.DizimistaId).ThenBy(o => o.AnoReferencia).ThenBy(o => o.MesReferencia)
        ofertas = [.. ofertasFiltradas
            .OrderByDescending(o => o.Data)
            .ThenBy(o => o.DizimistaId)
            .ThenBy(o => o.AnoReferencia)
            .ThenBy(o => o.MesReferencia)];

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
        headerRow.Cell(7).Value = "Tipo Pagamento";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dados
        decimal totalValor = 0;
        int rowNumber = 2;
        
        foreach (var o in ofertas)
        {
            var codigoDizimista = "";
            var nomeDizimista = "";

            if (dicionarioDizimistas.TryGetValue(o.DizimistaId, out var dizimista))
            {
                codigoDizimista = dizimista.NumeroCadastro.ToString();
                nomeDizimista = dizimista.Nome;
            }

            worksheet.Cell(rowNumber, 1).Value = codigoDizimista;
            worksheet.Cell(rowNumber, 2).Value = nomeDizimista;
            worksheet.Cell(rowNumber, 3).Value = o.Valor;
            worksheet.Cell(rowNumber, 4).Value = o.Data;
            worksheet.Cell(rowNumber, 5).Value = o.MesReferencia;
            worksheet.Cell(rowNumber, 6).Value = o.AnoReferencia;
            worksheet.Cell(rowNumber, 7).Value = o.TipoPagamento.ToString();

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
        worksheet.Column(7).Width = 18;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public static MemoryStream GerarModelo()
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
        headerRow.Cell(6).Value = "Tipo Pagamento";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:F1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Exemplo de linha
        worksheet.Cell(2, 1).Value = 123;
        worksheet.Cell(2, 2).Value = 100.00;
        worksheet.Cell(2, 3).Value = DateTime.Now;
        worksheet.Cell(2, 4).Value = 1;
        worksheet.Cell(2, 5).Value = DateTime.Now.Year;
        worksheet.Cell(2, 6).Value = "PIX";

        // Linhas em branco para o usuário preencher (5 linhas)
        for (int i = 3; i <= 7; i++)
        {
            for (int j = 1; j <= 6; j++)
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
        worksheet.Column(6).Width = 18;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public class ResultadoImportacao
    {
        public List<Oferta> OfertasImportadas { get; set; } = [.. new List<Oferta>()];
        public List<string> Erros { get; set; } = [.. new List<string>()];
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

            // Ler tipo de pagamento (coluna 6)
            var tipoPagamentoCell = worksheet.Cell(rowNumber, 6).GetValue<string>();
            TipoPagamento tipoPagamento = TipoPagamento.Dinheiro; // Default é Dinheiro

            if (!string.IsNullOrWhiteSpace(tipoPagamentoCell))
            {
                var tipoPagamentoTrimmed = tipoPagamentoCell.Trim();
                if (tipoPagamentoTrimmed.Equals("PIX", StringComparison.OrdinalIgnoreCase))
                    tipoPagamento = TipoPagamento.PIX;
                else if (tipoPagamentoTrimmed.Equals("Dinheiro", StringComparison.OrdinalIgnoreCase))
                    tipoPagamento = TipoPagamento.Dinheiro;
                else if (tipoPagamentoTrimmed.Equals("Cartão", StringComparison.OrdinalIgnoreCase) || 
                         tipoPagamentoTrimmed.Equals("Cartao", StringComparison.OrdinalIgnoreCase))
                    tipoPagamento = TipoPagamento.Cartao;
                else
                {
                    resultado.Erros.Add($"Linha {numeroLinha}: Tipo de pagamento '{tipoPagamentoCell.Trim()}' é inválido (use PIX, Dinheiro ou Cartão)");
                    numeroLinha++;
                    continue;
                }
            }

            resultado.OfertasImportadas.Add(new Oferta
            {
                Id = Guid.NewGuid(),
                DizimistaId = dizimista.Id,
                Valor = valor,
                Data = data,
                MesReferencia = mesReferencia,
                AnoReferencia = anoReferencia,
                TipoPagamento = tipoPagamento
            });

            numeroLinha++;
        }

        return resultado;
    }
}

