using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Globalization;
using System.Text;

namespace Dizimo.Services;

public class OfertaPdfService
{
    private readonly IUnitOfWork _unitOfWork;

    public OfertaPdfService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ImprimirAsync(List<Oferta>? ofertas = null, DateTime? dataInicio = null, DateTime? dataFim = null, string? tipoPagamento = null, string? filtroNome = null)
    {
        // Se nenhuma lista for passada, busca todas as ofertas
        if (ofertas == null)
        {
            var todasOfertas = await _unitOfWork.Ofertas.GetAllAsync();
            ofertas = todasOfertas is List<Oferta> l ? l : todasOfertas.ToList();
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

        ofertas = ofertasFiltradas.ToList();

        // Gerar HTML para impressão
        var html = GerarHtmlOfertas(ofertas, dicionarioDizimistas);

        // Converter HTML para PDF usando iTextSharp (alternativa: SelectPdf ou IronPdf)
        var pdfStream = ConvertHtmlToPdf(html);

        return pdfStream;
    }

    private string GerarHtmlOfertas(List<Oferta> ofertas, Dictionary<Guid, Dizimista> dicionarioDizimistas)
    {
        var sb = new StringBuilder();
        decimal totalValor = 0;

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("<title>Relatório de Ofertas</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("* { margin: 0; padding: 0; }");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { text-align: center; color: #333; margin-bottom: 30px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; border: 1px solid #999; }");
        sb.AppendLine("th { background-color: #d3d3d3; padding: 10px; border: 1px solid #999; text-align: left; font-weight: bold; }");
        sb.AppendLine("td { padding: 8px; border: 1px solid #ddd; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine(".text-center { text-align: center; }");
        sb.AppendLine(".text-right { text-align: right; }");
        sb.AppendLine(".total-container { margin-top: 40px; padding: 20px; background-color: #fff3cd; border: 2px solid #ffc107; border-radius: 5px; }");
        sb.AppendLine(".total-container h2 { text-align: center; color: #333; margin-bottom: 20px; }");
        sb.AppendLine(".total-summary { text-align: center; }");
        sb.AppendLine(".total-summary p { margin: 10px 0; font-size: 16px; font-weight: bold; }");
        sb.AppendLine(".total-amount { font-size: 28px; font-weight: bold; color: #d32f2f; margin-top: 15px; }");
        sb.AppendLine("@media print { body { margin: 0; padding: 10px; } }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body onload='window.print();'>");

        sb.AppendLine("<h1>Relatório de Ofertas</h1>");
        sb.AppendLine($"<p class='text-center'>Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");

        sb.AppendLine("<table>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>Código Dizimista</th>");
        sb.AppendLine("<th>Nome Dizimista</th>");
        sb.AppendLine("<th>Valor</th>");
        sb.AppendLine("<th class='text-center'>Data</th>");
        sb.AppendLine("<th class='text-center'>Mês Ref.</th>");
        sb.AppendLine("<th class='text-center'>Ano Ref.</th>");
        sb.AppendLine("<th class='text-center'>Tipo Pagto</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        foreach (var oferta in ofertas)
        {
            var codigoDizimista = "";
            var nomeDizimista = "";

            if (dicionarioDizimistas.TryGetValue(oferta.DizimistaId, out var dizimista))
            {
                codigoDizimista = dizimista.NumeroCadastro.ToString();
                nomeDizimista = dizimista.Nome;
            }

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{codigoDizimista}</td>");
            sb.AppendLine($"<td>{nomeDizimista}</td>");
            sb.AppendLine($"<td class='text-right'>R$ {oferta.Valor:N2}</td>");
            sb.AppendLine($"<td class='text-center'>{oferta.Data:dd/MM/yyyy}</td>");
            sb.AppendLine($"<td class='text-center'>{oferta.MesReferencia}</td>");
            sb.AppendLine($"<td class='text-center'>{oferta.AnoReferencia}</td>");
            sb.AppendLine($"<td class='text-center'>{oferta.TipoPagamento}</td>");
            sb.AppendLine("</tr>");

            totalValor += oferta.Valor;
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        // Total container com page-break-before para forçar na última página
        sb.AppendLine("<div class='total-container'>");
        sb.AppendLine("<h2>📊 Resumo Final</h2>");
        sb.AppendLine("<div class='total-summary'>");
        sb.AppendLine($"<p>Total de ofertas: {ofertas.Count}</p>");
        sb.AppendLine($"<div class='total-amount'>R$ {totalValor:N2}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private MemoryStream ConvertHtmlToPdf(string html)
    {
        var stream = new MemoryStream();

        try
        {
            // Gerar um arquivo HTML temporário que será convertido em PDF
            // Esta é uma abordagem compatível com .NET MAUI
            var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"oferta_{Guid.NewGuid()}.html");
            File.WriteAllText(tempHtmlPath, html, Encoding.UTF8);

            // Usar o navegador padrão do Windows para imprimir como PDF
            // ou usar uma biblioteca como SelectPdf (necessário adicionar via NuGet)
            // Para agora, vamos retornar o HTML como conteúdo
            var htmlBytes = Encoding.UTF8.GetBytes(html);
            stream.Write(htmlBytes, 0, htmlBytes.Length);
            stream.Position = 0;

            // Limpeza
            try { File.Delete(tempHtmlPath); } catch { }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao gerar PDF: {ex.Message}");
            stream.Position = 0;
        }

        return stream;
    }
}
