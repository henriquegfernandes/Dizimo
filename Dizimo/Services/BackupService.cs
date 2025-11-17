using System.Text.Json;
using Dizimo.Data;
using Dizimo.Models;

namespace Dizimo.Services;

public class BackupService
{
    private readonly DizimistaRepository _dizimistaRepo;
    private readonly OfertaRepository _ofertaRepo;

    public BackupService(DizimistaRepository dizimistaRepo, OfertaRepository ofertaRepo)
    {
        _dizimistaRepo = dizimistaRepo;
        _ofertaRepo = ofertaRepo;
    }

    public async Task<string> ExportJsonAsync(string path)
    {
        var dizimistas = await _dizimistaRepo.ListAsync();
        var ofertas = await _ofertaRepo.ListAsync();
        var payload = new { Dizimistas = dizimistas, Ofertas = ofertas };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
        return path;
    }

    public async Task<int> ImportJsonAsync(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Backup file not found", path);
        var text = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        var count = 0;

        // First, import dizimistas and build a map from original ID -> new ID (or existing ID if matched by code)
        var idMap = new Dictionary<int, int>();
        if (root.TryGetProperty("Dizimistas", out var dizArr))
        {
            foreach (var el in dizArr.EnumerateArray())
            {
                try
                {
                    var origId = el.TryGetProperty("ID", out var pid) ? pid.GetInt32() : 0;
                    var nome = el.GetProperty("Nome").GetString() ?? string.Empty;
                    var codigo = el.GetProperty("Codigo").GetString() ?? string.Empty;

                    // If a dizimista with same code exists, reuse; otherwise create new and map
                    var existing = string.IsNullOrWhiteSpace(codigo) ? null : await _dizimistaRepo.GetByCodigoAsync(codigo);
                    if (existing is not null)
                    {
                        if (origId != 0) idMap[origId] = existing.ID;
                        continue;
                    }

                    var newId = await _dizimistaRepo.SaveAsync(new Dizimista { Nome = nome, Codigo = codigo, DataNascimento = null });
                    if (origId != 0) idMap[origId] = newId;
                    count++;
                }
                catch { /* ignore invalid dizimista entries */ }
            }
        }

        // Then import ofertas, remapping DizimistaID when possible
        if (root.TryGetProperty("Ofertas", out var ofertasArr))
        {
            foreach (var el in ofertasArr.EnumerateArray())
            {
                try
                {
                    var dizId = el.GetProperty("DizimistaID").GetInt32();
                    var valor = el.GetProperty("Valor").GetDecimal();
                    var data = DateTime.Parse(el.GetProperty("Data").GetString());
                    var obs = el.TryGetProperty("Observacao", out var o) ? o.GetString() ?? string.Empty : string.Empty;

                    // Map original dizimista id -> new id if available
                    var targetId = dizId;
                    if (dizId != 0 && idMap.TryGetValue(dizId, out var mapped)) targetId = mapped;

                    // If still not found, try to match by Code if present in oferta object (optional)
                    if (targetId == 0 && el.TryGetProperty("DizimistaCodigo", out var codeProp))
                    {
                        var code = codeProp.GetString() ?? string.Empty;
                        var match = await _dizimistaRepo.GetByCodigoAsync(code);
                        if (match is not null) targetId = match.ID;
                    }

                    if (targetId == 0)
                    {
                        // cannot map, skip this oferta to avoid orphaning
                        continue;
                    }

                    await _ofertaRepo.SaveAsync(new Oferta { DizimistaID = targetId, Valor = valor, Data = data, Observacao = obs });
                }
                catch { /* ignore malformed entries */ }
            }
        }

        return count;
    }
}
