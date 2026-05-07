using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Dizimo.Domain.Entities;

namespace Dizimo.Application.Reporting.Services;

public class AniversariantesExcelService
{
    public MemoryStream Exportar(IEnumerable<Dizimista> aniversariantes)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Aniversariantes");

        // Cabeçalho
        worksheet.Cell(1, 1).Value = "Número Cadastro";
        worksheet.Cell(1, 2).Value = "Nome";
        worksheet.Cell(1, 3).Value = "Data de Nascimento";
        var headerRange = worksheet.Range("A1:C1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 2;
        foreach (var d in aniversariantes)
        {
            worksheet.Cell(row, 1).Value = d.NumeroCadastro;
            worksheet.Cell(row, 2).Value = d.Nome;
            worksheet.Cell(row, 3).Value = d.DataNascimento;
            worksheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";
            row++;
        }

        worksheet.Column(1).Width = 18;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 18;
        worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}

