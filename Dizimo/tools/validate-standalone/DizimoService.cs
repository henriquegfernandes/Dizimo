using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public Task<int> LançarOfertaAsync(int dizimistaId, decimal valor, DateTime data, string? observacao = null)
    {
        var oferta = new Oferta { DizimistaID = dizimistaId, Valor = valor, Data = data, Observacao = observacao ?? string.Empty };
        return _ofertaRepo.SaveAsync(oferta);
    }
}
