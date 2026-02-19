using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Text;

namespace Dizimo.Services;

public class AniversariantesPdfService(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<MemoryStream> ImprimirAsync(List<Dizimista>? aniversariantes = null)
    {
        // Se nenhuma lista for passada, busca todos os aniversariantes
        if (aniversariantes == null)
        {
            var todosDizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
            aniversariantes = todosDizimistas is List<Dizimista> l ? l : [.. todosDizimistas];
        }

        // Gerar HTML para impressão
        var html = GerarHtmlAniversariantes(aniversariantes);

        // Converter HTML para PDF
        var pdfStream = ConvertHtmlToPdf(html);

        return pdfStream;
    }

    private static string GerarHtmlAniversariantes(List<Dizimista> aniversariantes)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("<title>Relatório de Aniversariantes</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { text-align: center; color: #333; margin-bottom: 10px; }");
        sb.AppendLine("h2 { text-align: center; color: #666; font-size: 16px; margin-bottom: 30px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; border: 1px solid #999; }");
        sb.AppendLine("th { background-color: #4CAF50; color: white; padding: 12px; border: 1px solid #999; text-align: left; font-weight: bold; }");
        sb.AppendLine("td { padding: 10px; border: 1px solid #ddd; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine("tr:hover { background-color: #f0f0f0; }");
        sb.AppendLine(".text-center { text-align: center; }");
        sb.AppendLine(".summary-birthday { margin-top: 40px; padding: 20px; background-color: #fff3e0; border: 2px solid #ff9800; border-radius: 5px; }");
        sb.AppendLine(".summary-birthday h2 { text-align: center; color: #333; margin: 0 0 20px 0; font-size: 20px; }");
        sb.AppendLine(".birthday-content { text-align: center; }");
        sb.AppendLine(".birthday-content p { margin: 10px 0; font-size: 16px; font-weight: bold; }");
        sb.AppendLine(".birthday-total { font-size: 28px; color: #e65100; margin-top: 15px; }");
        sb.AppendLine("@media print { body { margin: 0; padding: 10px; } }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body onload='window.print();'>");

        sb.AppendLine("<h1>Relatório de Aniversariantes</h1>");
        sb.AppendLine($"<h2>Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}</h2>");

        if (aniversariantes == null || aniversariantes.Count == 0)
        {
            sb.AppendLine("<p class='text-center'>Nenhum aniversariante para exibir.</p>");
        }
        else
        {
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Código</th>");
            sb.AppendLine("<th>Nome</th>");
            sb.AppendLine("<th class='text-center'>Data Nascimento</th>");
            sb.AppendLine("<th class='text-center'>Idade</th>");
            sb.AppendLine("<th class='text-center'>Telefone</th>");
            sb.AppendLine("<th class='text-center'>WhatsApp</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var dizimista in aniversariantes)
            {
                var idade = CalcularIdade(dizimista.DataNascimento);

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{dizimista.NumeroCadastro}</td>");
                sb.AppendLine($"<td>{dizimista.Nome}</td>");
                sb.AppendLine($"<td class='text-center'>{dizimista.DataNascimento:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td class='text-center'>{idade} anos</td>");
                sb.AppendLine($"<td class='text-center'>{dizimista.Telefone}</td>");
                sb.AppendLine($"<td class='text-center'>{dizimista.Whatsapp}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Summary container com page-break-before para forçar na última página
            sb.AppendLine("<div class='summary-birthday'>");
            sb.AppendLine("<h2>🎂 Resumo Final</h2>");
            sb.AppendLine("<div class='birthday-content'>");
            sb.AppendLine($"<p>Total de aniversariantes: {aniversariantes.Count}</p>");
            sb.AppendLine($"<div class='birthday-total'>Parabéns a todos!</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>Total de aniversariantes: {aniversariantes.Count}</p>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static int CalcularIdade(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    private static MemoryStream ConvertHtmlToPdf(string html)
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
