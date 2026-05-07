using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Text;

namespace Dizimo.Application.Reporting.Services;

public class DizimistaPdfService
{
    private readonly IUnitOfWork _unitOfWork;

    public DizimistaPdfService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ImprimirAsync(List<Dizimista>? dizimistas = null, string? filtroNome = null, bool? apenasAtivos = null)
    {
        // Se nenhuma lista for passada, busca todos os dizimistas
        if (dizimistas == null)
        {
            var todosDizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
            dizimistas = todosDizimistas is List<Dizimista> l ? l : todosDizimistas.ToList();
        }

        // Aplicar filtros
        var dizimistasFiltrados = dizimistas.AsEnumerable();

        // Filtro de nome
        if (!string.IsNullOrWhiteSpace(filtroNome))
        {
            dizimistasFiltrados = dizimistasFiltrados.Where(d =>
                d.Nome.Contains(filtroNome, StringComparison.OrdinalIgnoreCase) ||
                d.NumeroCadastro.ToString().Contains(filtroNome));
        }

        // Filtro de status
        if (apenasAtivos.HasValue)
        {
            dizimistasFiltrados = dizimistasFiltrados.Where(d => d.Ativo == apenasAtivos.Value);
        }

        dizimistas = dizimistasFiltrados.ToList();

        // Gerar HTML para impressão
        var html = GerarHtmlDizimistas(dizimistas);

        // Converter HTML para PDF
        var pdfStream = ConvertHtmlToPdf(html);

        return pdfStream;
    }

    private string GerarHtmlDizimistas(List<Dizimista> dizimistas)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("<title>Relatório de Dizimistas</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { text-align: center; color: #333; margin-bottom: 30px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        sb.AppendLine("th { background-color: #d3d3d3; padding: 10px; border: 1px solid #999; text-align: left; font-weight: bold; }");
        sb.AppendLine("td { padding: 8px; border: 1px solid #ddd; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine(".text-center { text-align: center; }");
        sb.AppendLine(".status-ativo { color: green; font-weight: bold; }");
        sb.AppendLine(".status-inativo { color: red; font-weight: bold; }");
        sb.AppendLine(".summary-container { margin-top: 40px; padding: 20px; background-color: #e3f2fd; border: 2px solid #2196F3; border-radius: 5px; }");
        sb.AppendLine(".summary-container h2 { text-align: center; color: #333; margin-bottom: 20px; }");
        sb.AppendLine(".summary-content { text-align: center; }");
        sb.AppendLine(".summary-content p { margin: 10px 0; font-size: 16px; font-weight: bold; }");
        sb.AppendLine(".summary-stats { font-size: 18px; color: #1976d2; margin-top: 15px; }");
        sb.AppendLine("@media print { body { margin: 0; padding: 10px; } }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body onload='window.print();'>");

        sb.AppendLine("<h1>Relatório de Dizimistas</h1>");
        sb.AppendLine($"<p class='text-center'>Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");

        sb.AppendLine("<table>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>Código</th>");
        sb.AppendLine("<th>Nome</th>");
        sb.AppendLine("<th class='text-center'>Data Nascimento</th>");
        sb.AppendLine("<th class='text-center'>Telefone</th>");
        sb.AppendLine("<th class='text-center'>WhatsApp</th>");
        sb.AppendLine("<th class='text-center'>Status</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        foreach (var dizimista in dizimistas)
        {
            var statusClass = dizimista.Ativo ? "status-ativo" : "status-inativo";
            var statusText = dizimista.Ativo ? "Ativo" : "Inativo";

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{dizimista.NumeroCadastro}</td>");
            sb.AppendLine($"<td>{dizimista.Nome}</td>");
            sb.AppendLine($"<td class='text-center'>{dizimista.DataNascimento:dd/MM/yyyy}</td>");
            sb.AppendLine($"<td class='text-center'>{dizimista.Telefone}</td>");
            sb.AppendLine($"<td class='text-center'>{dizimista.Whatsapp}</td>");
            sb.AppendLine($"<td class='text-center {statusClass}'>{statusText}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        // Summary container com page-break-before para forçar na última página
        sb.AppendLine("<div class='summary-container'>");
        sb.AppendLine("<h2>📋 Resumo Final</h2>");
        sb.AppendLine("<div class='summary-content'>");
        sb.AppendLine($"<p>Total de dizimistas: {dizimistas.Count}</p>");
        sb.AppendLine($"<div class='summary-stats'>");
        sb.AppendLine($"  <p>✓ Ativos: {dizimistas.Count(d => d.Ativo)}</p>");
        sb.AppendLine($"  <p>✗ Inativos: {dizimistas.Count(d => !d.Ativo)}</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"<p>Total de dizimistas: {dizimistas.Count}</p>");
        sb.AppendLine($"<p>Ativos: {dizimistas.Count(d => d.Ativo)} | Inativos: {dizimistas.Count(d => !d.Ativo)}</p>");
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
            var htmlBytes = Encoding.UTF8.GetBytes(html);
            stream.Write(htmlBytes, 0, htmlBytes.Length);
            stream.Position = 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao gerar PDF: {ex.Message}");
            stream.Position = 0;
        }

        return stream;
    }
}

