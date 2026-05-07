using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dizimo.Application.Dashboard;

public class DashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public class DizimistaPeriodoOfertaData
    {
        public string Periodo { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Cor { get; set; } = string.Empty;

        public DizimistaPeriodoOfertaData() { }

        public DizimistaPeriodoOfertaData(string periodo, int quantidade, string cor)
        {
            Periodo = periodo;
            Quantidade = quantidade;
            Cor = cor;
        }
    }

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Retorna estatísticas de dizimistas agrupados por período da última oferta
    /// Verde: Últimos 2 meses
    /// Amarelo: 2 a 6 meses
    /// Laranja: 6 meses a 1 ano
    /// Vermelho: mais de 1 ano
    /// </summary>
    public async Task<List<DizimistaPeriodoOfertaData>> GetDizimistasAgrupadosPorPeriodoAsync()
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var ofertas = await _unitOfWork.Ofertas.GetAllAsync();

        // Filtrar apenas dizimistas ativos
        var dizimistasAtivos = dizimistas.Where(d => d.Ativo).ToList();

        var hoje = DateTime.Today;
        var dados = new List<DizimistaPeriodoOfertaData>();

        // Agrupar dizimistas por período da última oferta
        var dizimistasPorPeriodo = new Dictionary<string, int>
        {
            { "Últimos 2 meses", 0 },
            { "2-6 meses", 0 },
            { "6-12 meses", 0 },
            { "Mais de 1 ano", 0 }
        };

        foreach (var dizimista in dizimistasAtivos)
        {
            var ultimaOferta = ofertas
                .Where(o => o.DizimistaId == dizimista.Id)
                .OrderByDescending(o => o.Data)
                .FirstOrDefault();

            if (ultimaOferta == null)
            {
                // Dizimista sem oferta registrada
                dizimistasPorPeriodo["Mais de 1 ano"]++;
                continue;
            }

            var diasDesdeOferta = (hoje - ultimaOferta.Data).TotalDays;
            var mesesDesdeOferta = diasDesdeOferta / 30.0;

            if (mesesDesdeOferta <= 2)
                dizimistasPorPeriodo["Últimos 2 meses"]++;
            else if (mesesDesdeOferta <= 6)
                dizimistasPorPeriodo["2-6 meses"]++;
            else if (mesesDesdeOferta <= 12)
                dizimistasPorPeriodo["6-12 meses"]++;
            else
                dizimistasPorPeriodo["Mais de 1 ano"]++;
        }

        // Mapear para cores
        dados.Add(new("Últimos 2 meses", dizimistasPorPeriodo["Últimos 2 meses"], "#22C55E")); // Verde
        dados.Add(new("2-6 meses", dizimistasPorPeriodo["2-6 meses"], "#FBBF24")); // Amarelo
        dados.Add(new("6-12 meses", dizimistasPorPeriodo["6-12 meses"], "#F97316")); // Laranja
        dados.Add(new("Mais de 1 ano", dizimistasPorPeriodo["Mais de 1 ano"], "#EF4444")); // Vermelho

        return dados;
    }

    /// <summary>
    /// Retorna dizimistas aniversariantes da semana atual
    /// Considera dizimistas que completam aniversário do sábado passado até o final da semana atual
    /// </summary>
    public async Task<List<Dizimista>> GetAniversariantesSemanasAsync()
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var hoje = DateTime.Today;

        // Descobrir o sábado passado
        var diasDesdeSegunda = (int)hoje.DayOfWeek - 1;
        if (diasDesdeSegunda < 0) diasDesdeSegunda += 7;
        
        var sabadoPassado = hoje.AddDays(-diasDesdeSegunda - 2);
        if (sabadoPassado > hoje)
            sabadoPassado = sabadoPassado.AddDays(-7);

        var proximoSabado = sabadoPassado.AddDays(7);

        return dizimistas
            .Where(d => d.Ativo)
            .Where(d =>
            {
                var mesAno = new DateTime(hoje.Year, d.DataNascimento.Month, d.DataNascimento.Day);
                return mesAno >= sabadoPassado && mesAno <= proximoSabado;
            })
            .OrderBy(d => d.DataNascimento.Month)
            .ThenBy(d => d.DataNascimento.Day)
            .ThenBy(d => d.Nome)
            .ToList();
    }

    /// <summary>
    /// Retorna dizimistas aniversariantes do mês atual
    /// </summary>
    public async Task<List<Dizimista>> GetAniversariantesMesAsync(int? month)
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAllAsync();
        var today = DateTime.Today;
        var referenceMonth = month is null ? today.Month : month;

        return dizimistas
            .Where(d => d.DataNascimento.Month == referenceMonth && d.Ativo == true)
            .OrderBy(d => d.DataNascimento.Day)
            .ThenBy(d => d.Nome)
            .ToList();
    }
}
