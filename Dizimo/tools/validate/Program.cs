using System;
using System.IO;
using Dizimo.Data;
using Dizimo.Models;
using Dizimo.Services;
using Microsoft.Extensions.Logging;

// Simple runner to validate repositories/services compile and basic operations.
Console.WriteLine("Starting validation runner...");

var tempDb = Path.Combine(Path.GetTempPath(), "Dizimo_Validate.db3");
if (File.Exists(tempDb)) File.Delete(tempDb);

// Replace Constants.DatabasePath via reflection hack: create a local connection string environment
// Simpler approach: set environment variable to indicate use of temp DB and update Constants class for test.
Environment.SetEnvironmentVariable("DIZIMO_TEST_DB", tempDb);

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var dizimistaRepo = new DizimistaRepository(loggerFactory.CreateLogger<DizimistaRepository>());
var ofertaRepo = new OfertaRepository(loggerFactory.CreateLogger<OfertaRepository>());
var service = new DizimoService(dizimistaRepo, ofertaRepo);

// Create and save a dizimista
var d = new Dizimista { Nome = "João Silva", Codigo = "001", Ativo = true };
var id = await dizimistaRepo.SaveAsync(d);
Console.WriteLine($"Dizimista criado ID={id}");

// Lançar uma oferta
var ofertaId = await service.LançarOfertaAsync(id, 50.0m, DateTime.Now, "Ofertar para manutenção");
Console.WriteLine($"Oferta criada ID={ofertaId}");

// Buscar ofertas por dizimista
var ofertas = await ofertaRepo.SearchByDizimistaIdAsync(id);
Console.WriteLine($"Ofertas encontradas para dizimista {id}: {ofertas.Count}");

Console.WriteLine("Validation runner finished.");
