using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting standalone validation...");
// Print the actual connection string/path used by the tool (helps locate the DB file)
Console.WriteLine(Constants.DatabasePath);

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var dizimistaRepo = new DizimistaRepository(loggerFactory.CreateLogger("dizimista"));
var ofertaRepo = new OfertaRepository(loggerFactory.CreateLogger("oferta"));
var service = new DizimoService(dizimistaRepo, ofertaRepo);

var d = new Dizimista { Nome = "Maria", Codigo = "100" };
var id = await dizimistaRepo.SaveAsync(d);
Console.WriteLine($"Dizimista salvo com ID={id}");

var ofertaId = await service.LançarOfertaAsync(id, 25.5m, DateTime.Now, "Domingo");
Console.WriteLine($"Oferta criada ID={ofertaId}");

var ofertas = await ofertaRepo.SearchByDizimistaIdAsync(id);
Console.WriteLine($"Ofertas por dizimista: {ofertas.Count}");

// Export backup JSON to temp
var backupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dizimo_backup_{DateTime.Now:yyyyMMddHHmmss}.json");
var dizimistas = await dizimistaRepo.ListAsync();
// export all ofertas for simplicity
var allOfertas = new List<object>();
foreach (var dz in dizimistas)
{
    var ofertasFor = await ofertaRepo.SearchByDizimistaIdAsync(dz.ID);
    foreach (var o in ofertasFor)
    {
        allOfertas.Add(new { o.ID, o.DizimistaID, DizimistaCodigo = dz.Codigo, o.Valor, Data = o.Data.ToString("o"), o.Observacao });
    }
}

var payload = new { Dizimistas = dizimistas.Select(d => new { d.ID, d.Nome, d.Codigo }), Ofertas = allOfertas };
await System.IO.File.WriteAllTextAsync(backupPath, System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Backup exported to: {backupPath}");

Console.WriteLine("Standalone validation finished.");
// Optional demo import: only run if VALIDATE_DO_IMPORT is set to "1"
if (Environment.GetEnvironmentVariable("VALIDATE_DO_IMPORT") == "1")
{
    // Demonstrate import: read back the file and re-insert as new entries under a different DB filename
    var importDb = Path.Combine(Path.GetTempPath(), "ValidateImport.db3");
    Environment.SetEnvironmentVariable("VALIDATE_DB_FILENAME", Path.GetFileName(importDb));
    var importDizimistaRepo = new DizimistaRepository(loggerFactory.CreateLogger("import-dizimista"));
    var importOfertaRepo = new OfertaRepository(loggerFactory.CreateLogger("import-oferta"));
    var json = await System.IO.File.ReadAllTextAsync(backupPath);
    var doc = System.Text.Json.JsonDocument.Parse(json);
    var root = doc.RootElement;
    int imported = 0;
    if (root.TryGetProperty("Dizimistas", out var dizArr))
    {
        foreach (var el in dizArr.EnumerateArray())
        {
            var nome = el.GetProperty("Nome").GetString() ?? "";
            var codigo = el.GetProperty("Codigo").GetString() ?? "";
            var nd = new Dizimista { Nome = nome, Codigo = codigo };
            await importDizimistaRepo.SaveAsync(nd);
            imported++;
        }
    }
    Console.WriteLine($"Imported {imported} dizimistas into {importDb}");
}
else
{
    Console.WriteLine("Skipping demo import (set VALIDATE_DO_IMPORT=1 to enable)");
}
