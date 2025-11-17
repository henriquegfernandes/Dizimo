using Dizimo.Data;
using Dizimo.Models;

namespace Dizimo.Services;

public class DizimoService
{
    private readonly DizimistaRepository _dizimistaRepo;
    private readonly OfertaRepository _ofertaRepo;

    public DizimoService(DizimistaRepository dizimistaRepo, OfertaRepository ofertaRepo)
    {
        _dizimistaRepo = dizimistaRepo;
        _ofertaRepo = ofertaRepo;
    }

    public Task<List<Dizimista>> ListDizimistasAsync() => _dizimistaRepo.ListAsync();

    public Task<Dizimista?> GetDizimistaByCodigoAsync(string codigo) => _dizimistaRepo.GetByCodigoAsync(codigo);

    public Task<int> SaveDizimistaAsync(Dizimista d) => _dizimistaRepo.SaveAsync(d);

    public Task<int> InativarDizimistaAsync(int id)
    {
        return _dizimistaRepo.GetByIdAsync(id) is null ? Task.FromResult(0) : InativarImpl(id);
    }

    private async Task<int> InativarImpl(int id)
    {
        var d = await _dizimistaRepo.GetByIdAsync(id);
        if (d is null) return 0;
        d.Ativo = false;
        return await _dizimistaRepo.SaveAsync(d);
    }

    public Task<int> LançarOfertaAsync(int dizimistaId, decimal valor, DateTime data, string? observacao = null)
    {
        var oferta = new Oferta { DizimistaID = dizimistaId, Valor = valor, Data = data, Observacao = observacao ?? string.Empty };
        return _ofertaRepo.SaveAsync(oferta);
    }

    public Task<List<Oferta>> BuscarOfertasPorDataAsync(DateTime date) => _ofertaRepo.ListByDateAsync(date);

    public Task<List<Oferta>> BuscarOfertasPorDizimistaAsync(int dizimistaId) => _ofertaRepo.SearchByDizimistaIdAsync(dizimistaId);

    // Reports
    public async Task<List<(Dizimista Dizimista, int Count, decimal Total)>> RelatorioGeralAsync()
    {
        var diz = await _dizimistaRepo.ListAsync();
        var result = new List<(Dizimista, int, decimal)>();
        foreach (var d in diz)
        {
            var ofertas = await _ofertaRepo.SearchByDizimistaIdAsync(d.ID);
            var total = ofertas.Sum(o => o.Valor);
            result.Add((d, ofertas.Count, total));
        }
        return result;
    }

    public async Task<List<Dizimista>> RelatorioAniversariantesAsync(int mes)
    {
        var diz = await _dizimistaRepo.ListAsync();
        return diz.Where(d => d.DataNascimento?.Month == mes).ToList();
    }

    public Task<List<Oferta>> RelatorioOfertasPorDataAsync(DateTime date) => _ofertaRepo.ListByDateAsync(date);
}
