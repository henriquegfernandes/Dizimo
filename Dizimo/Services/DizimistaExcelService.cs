using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using ClosedXML.Excel;
using System.Globalization;

namespace Dizimo.Services;

public class DizimistaExcelService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public DizimistaExcelService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ExportarAsync(string? filtroNome = null, string? statusSelecionado = null)
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();

        // Aplicar filtros
        var dizimistasFiltrados = dizimistas.AsEnumerable();

        // Filtro de status
        if (!string.IsNullOrWhiteSpace(statusSelecionado) && statusSelecionado != "Todos")
        {
            if (statusSelecionado == "Ativos")
                dizimistasFiltrados = dizimistasFiltrados.Where(d => d.Ativo);
            else if (statusSelecionado == "Inativos")
                dizimistasFiltrados = dizimistasFiltrados.Where(d => !d.Ativo);
        }

        // Filtro de nome
        if (!string.IsNullOrWhiteSpace(filtroNome))
        {
            dizimistasFiltrados = dizimistasFiltrados.Where(d =>
                d.Nome.Contains(filtroNome, StringComparison.OrdinalIgnoreCase) ||
                d.NumeroCadastro.ToString().Contains(filtroNome));
        }

        dizimistas = dizimistasFiltrados.ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Dizimistas");

        // Cabeçalho
        var headerRow = worksheet.Row(1);
        headerRow.Cell(1).Value = "Número Cadastro";
        headerRow.Cell(2).Value = "Nome";
        headerRow.Cell(3).Value = "Data Nascimento";
        headerRow.Cell(4).Value = "Telefone";
        headerRow.Cell(5).Value = "WhatsApp";
        headerRow.Cell(6).Value = "Data Cadastro";
        headerRow.Cell(7).Value = "Ativo";
        headerRow.Cell(8).Value = "Rua";
        headerRow.Cell(9).Value = "Número";
        headerRow.Cell(10).Value = "Complemento";
        headerRow.Cell(11).Value = "Bairro";
        headerRow.Cell(12).Value = "Cidade";
        headerRow.Cell(13).Value = "UF";
        headerRow.Cell(14).Value = "CEP";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:N1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dados
        int rowNumber = 2;
        foreach (var d in dizimistas)
        {
            var endereco = d.Endereco ?? new Endereco();

            worksheet.Cell(rowNumber, 1).Value = d.NumeroCadastro;
            worksheet.Cell(rowNumber, 2).Value = d.Nome;
            worksheet.Cell(rowNumber, 3).Value = d.DataNascimento;
            worksheet.Cell(rowNumber, 4).Value = d.Telefone;
            worksheet.Cell(rowNumber, 5).Value = d.Whatsapp;
            worksheet.Cell(rowNumber, 6).Value = d.DataCadastro;
            worksheet.Cell(rowNumber, 7).Value = d.Ativo ? "Sim" : "Não";
            worksheet.Cell(rowNumber, 8).Value = endereco.Rua;
            worksheet.Cell(rowNumber, 9).Value = endereco.Numero;
            worksheet.Cell(rowNumber, 10).Value = endereco.Complemento;
            worksheet.Cell(rowNumber, 11).Value = endereco.Bairro;
            worksheet.Cell(rowNumber, 12).Value = endereco.Cidade;
            worksheet.Cell(rowNumber, 13).Value = endereco.UF;
            worksheet.Cell(rowNumber, 14).Value = endereco.CEP;

            rowNumber++;
        }

        // Ajustar largura das colunas e formatos
        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 18;
        worksheet.Column(3).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 18;
        worksheet.Column(6).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(7).Width = 10;
        worksheet.Column(8).Width = 20;
        worksheet.Column(9).Width = 10;
        worksheet.Column(10).Width = 15;
        worksheet.Column(11).Width = 15;
        worksheet.Column(12).Width = 15;
        worksheet.Column(13).Width = 8;
        worksheet.Column(14).Width = 12;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public MemoryStream GerarModelo()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Dizimistas");

        // Cabeçalho
        var headerRow = worksheet.Row(1);
        headerRow.Cell(1).Value = "Número Cadastro";
        headerRow.Cell(2).Value = "Nome";
        headerRow.Cell(3).Value = "Data Nascimento";
        headerRow.Cell(4).Value = "Telefone";
        headerRow.Cell(5).Value = "WhatsApp";
        headerRow.Cell(6).Value = "Data Cadastro";
        headerRow.Cell(7).Value = "Ativo";
        headerRow.Cell(8).Value = "Rua";
        headerRow.Cell(9).Value = "Número";
        headerRow.Cell(10).Value = "Complemento";
        headerRow.Cell(11).Value = "Bairro";
        headerRow.Cell(12).Value = "Cidade";
        headerRow.Cell(13).Value = "UF";
        headerRow.Cell(14).Value = "CEP";

        // Formatar cabeçalho
        var headerRange = worksheet.Range("A1:N1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Exemplo de linha preenchida
        worksheet.Cell(2, 1).Value = 123;
        worksheet.Cell(2, 2).Value = "João da Silva";
        worksheet.Cell(2, 3).Value = DateTime.Parse("1990-05-15");
        worksheet.Cell(2, 4).Value = "11987654321";
        worksheet.Cell(2, 5).Value = "11987654321";
        worksheet.Cell(2, 6).Value = DateTime.Now;
        worksheet.Cell(2, 7).Value = "Sim";
        worksheet.Cell(2, 8).Value = "Rua das Flores";
        worksheet.Cell(2, 9).Value = "123";
        worksheet.Cell(2, 10).Value = "Apto 42";
        worksheet.Cell(2, 11).Value = "Centro";
        worksheet.Cell(2, 12).Value = "São Paulo";
        worksheet.Cell(2, 13).Value = "SP";
        worksheet.Cell(2, 14).Value = "01310100";

        // Linhas em branco para o usuário preencher (5 linhas)
        for (int i = 3; i <= 7; i++)
        {
            for (int j = 1; j <= 14; j++)
            {
                worksheet.Cell(i, j).Value = "";
            }
        }

        // Ajustar largura das colunas e formatos
        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 18;
        worksheet.Column(3).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 15;
        worksheet.Column(6).Width = 18;
        worksheet.Column(6).Style.DateFormat.Format = "dd/mm/yyyy";
        worksheet.Column(7).Width = 10;
        worksheet.Column(8).Width = 20;
        worksheet.Column(9).Width = 10;
        worksheet.Column(10).Width = 15;
        worksheet.Column(11).Width = 15;
        worksheet.Column(12).Width = 15;
        worksheet.Column(13).Width = 8;
        worksheet.Column(14).Width = 12;

        // Converter para MemoryStream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public async Task<List<Dizimista>> ImportarAsync(byte[] excelBytes)
    {
        var result = new List<Dizimista>();

        using var stream = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        int lastRow = 1;
        var lastRowUsed = worksheet.LastRowUsed();
        if (lastRowUsed != null)
        {
            lastRow = lastRowUsed.RowNumber();
        }

        for (int rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var numeroCadastroCell = worksheet.Cell(rowNumber, 1).GetValue<string>();
            if (string.IsNullOrWhiteSpace(numeroCadastroCell)) continue;

            if (!int.TryParse(numeroCadastroCell.Trim(), out var numeroCadastro)) continue;

            var nomeCell = worksheet.Cell(rowNumber, 2).GetValue<string>();
            var nome = nomeCell?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nome)) continue;

            var dataNascimentoCell = worksheet.Cell(rowNumber, 3).GetValue<string>();
            if (!DateTime.TryParseExact(dataNascimentoCell?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNascimento))
            {
                if (!DateTime.TryParse(dataNascimentoCell?.Trim(), out dataNascimento))
                    continue;
            }

            var telefone = worksheet.Cell(rowNumber, 4).GetValue<string>() ?? "";
            var whatsapp = worksheet.Cell(rowNumber, 5).GetValue<string>() ?? "";

            var dataCadastro = DateTime.Today;
            var dataCadastroCell = worksheet.Cell(rowNumber, 6).GetValue<string>();
            if (!string.IsNullOrWhiteSpace(dataCadastroCell))
            {
                if (DateTime.TryParseExact(dataCadastroCell.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dc))
                    dataCadastro = dc;
                else if (DateTime.TryParse(dataCadastroCell.Trim(), out var dc2))
                    dataCadastro = dc2;
            }

            var ativoCell = worksheet.Cell(rowNumber, 7).GetValue<string>();
            var ativo = !string.IsNullOrWhiteSpace(ativoCell) && (
                ativoCell.Equals("Sim", StringComparison.OrdinalIgnoreCase) ||
                ativoCell.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                ativoCell.Equals("1"));

            var endereco = new Endereco
            {
                Rua = worksheet.Cell(rowNumber, 8).GetValue<string>() ?? "",
                Numero = worksheet.Cell(rowNumber, 9).GetValue<string>() ?? "",
                Complemento = worksheet.Cell(rowNumber, 10).GetValue<string>() ?? "",
                Bairro = worksheet.Cell(rowNumber, 11).GetValue<string>() ?? "",
                Cidade = worksheet.Cell(rowNumber, 12).GetValue<string>() ?? "",
                UF = worksheet.Cell(rowNumber, 13).GetValue<string>() ?? "",
                CEP = worksheet.Cell(rowNumber, 14).GetValue<string>() ?? ""
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
}
