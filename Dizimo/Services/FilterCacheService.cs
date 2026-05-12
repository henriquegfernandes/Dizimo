using System.Diagnostics;

namespace Dizimo.Services;

/// <summary>
///     Serviço para gerenciar cache de filtros das páginas de listagem.
///     Permite restaurar filtros ao voltar na navegação e limpar ao navegar via menu.
/// </summary>
public interface IFilterCacheService
{
    /// <summary>
    ///     Salva os filtros da página de listagem de ofertas
    /// </summary>
    void SaveOfertaListFilters(DateTime? dataInicio, DateTime? dataFim, string nome, string tipoPagamento);

    /// <summary>
    ///     Restaura os filtros da página de listagem de ofertas
    /// </summary>
    (DateTime? dataInicio, DateTime? dataFim, string nome, string tipoPagamento)? GetOfertaListFilters();

    /// <summary>
    ///     Limpa os filtros da página de listagem de ofertas
    /// </summary>
    void ClearOfertaListFilters();

    /// <summary>
    ///     Salva os filtros da página de listagem de dizimistas
    /// </summary>
    void SaveDizimistaListFilters(string status, string nome);

    /// <summary>
    ///     Restaura os filtros da página de listagem de dizimistas
    /// </summary>
    (string status, string nome)? GetDizimistaListFilters();

    /// <summary>
    ///     Limpa os filtros da página de listagem de dizimistas
    /// </summary>
    void ClearDizimistaListFilters();
}

public class FilterCacheService : IFilterCacheService
{
    private const string OfertaDataInicioKey = "oferta_list_data_inicio";
    private const string OfertaDataFimKey = "oferta_list_data_fim";
    private const string OfertaNomeKey = "oferta_list_nome";
    private const string OfertaTipoPagamentoKey = "oferta_list_tipo_pagamento";

    private const string DizimistaStatusKey = "dizimista_list_status";
    private const string DizimistaNomeKey = "dizimista_list_nome";

    private const string DefaultTipoPagamento = "Todos";
    private const string DateFormat = "yyyy-MM-dd";
    private readonly IPreferencesService _preferencesService;

    public FilterCacheService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));
    }

    public void SaveOfertaListFilters(DateTime? dataInicio, DateTime? dataFim, string nome, string tipoPagamento)
    {
        _preferencesService.Set(OfertaDataInicioKey, dataInicio?.ToString(DateFormat) ?? string.Empty);
        _preferencesService.Set(OfertaDataFimKey, dataFim?.ToString(DateFormat) ?? string.Empty);
        _preferencesService.Set(OfertaNomeKey, nome ?? string.Empty);
        _preferencesService.Set(OfertaTipoPagamentoKey, tipoPagamento ?? DefaultTipoPagamento);

        Debug.WriteLine("[FilterCache] Filtros de ofertas salvos em cache");
    }

    public (DateTime? dataInicio, DateTime? dataFim, string nome, string tipoPagamento)? GetOfertaListFilters()
    {
        try
        {
            var dataInicio = _preferencesService.Get(OfertaDataInicioKey, string.Empty);
            var dataFim = _preferencesService.Get(OfertaDataFimKey, string.Empty);
            var nome = _preferencesService.Get(OfertaNomeKey, string.Empty);
            var tipoPagamento = _preferencesService.Get(OfertaTipoPagamentoKey, DefaultTipoPagamento);

            // Se nenhum filtro foi salvo, retorna null
            if (string.IsNullOrEmpty(dataInicio) && string.IsNullOrEmpty(dataFim) &&
                string.IsNullOrEmpty(nome) && tipoPagamento == DefaultTipoPagamento)
                return null;

            var parsedDataInicio = string.IsNullOrEmpty(dataInicio) ? (DateTime?)null : DateTime.Parse(dataInicio);
            var parsedDataFim = string.IsNullOrEmpty(dataFim) ? (DateTime?)null : DateTime.Parse(dataFim);

            Debug.WriteLine("[FilterCache] Filtros de ofertas restaurados do cache");
            return (parsedDataInicio, parsedDataFim, nome ?? string.Empty, tipoPagamento ?? DefaultTipoPagamento);
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"[FilterCache] Erro ao restaurar filtros de ofertas: {ex.Message}");
            return null;
        }
    }

    public void ClearOfertaListFilters()
    {
        _preferencesService.Remove(OfertaDataInicioKey);
        _preferencesService.Remove(OfertaDataFimKey);
        _preferencesService.Remove(OfertaNomeKey);
        _preferencesService.Remove(OfertaTipoPagamentoKey);

        Debug.WriteLine("[FilterCache] Filtros de ofertas limpos do cache");
    }

    public void SaveDizimistaListFilters(string status, string nome)
    {
        _preferencesService.Set(DizimistaStatusKey, status ?? string.Empty);
        _preferencesService.Set(DizimistaNomeKey, nome ?? string.Empty);

        Debug.WriteLine("[FilterCache] Filtros de dizimistas salvos em cache");
    }

    public (string status, string nome)? GetDizimistaListFilters()
    {
        try
        {
            var status = _preferencesService.Get(DizimistaStatusKey, string.Empty);
            var nome = _preferencesService.Get(DizimistaNomeKey, string.Empty);

            // Se nenhum filtro foi salvo, retorna null
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(nome)) return null;

            Debug.WriteLine("[FilterCache] Filtros de dizimistas restaurados do cache");
            return (status ?? DefaultTipoPagamento, nome ?? string.Empty);
        }
        catch (FormatException ex)
        {
            Debug.WriteLine($"[FilterCache] Erro ao restaurar filtros de dizimistas: {ex.Message}");
            return null;
        }
    }

    public void ClearDizimistaListFilters()
    {
        _preferencesService.Remove(DizimistaStatusKey);
        _preferencesService.Remove(DizimistaNomeKey);

        Debug.WriteLine("[FilterCache] Filtros de dizimistas limpos do cache");
    }
}