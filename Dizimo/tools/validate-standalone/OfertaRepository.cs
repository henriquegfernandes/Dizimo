using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OfertaRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public OfertaRepository(ILogger logger)
    {
        _logger = logger;
    }

    private async Task Init()
    {
        if (_hasBeenInitialized) return;
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Oferta (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                DizimistaID INTEGER NOT NULL,
                Valor REAL NOT NULL,
                Data TEXT NOT NULL,
                Observacao TEXT
            );";
        await createTableCmd.ExecuteNonQueryAsync();
        _hasBeenInitialized = true;
    }

    public async Task<int> SaveAsync(Oferta item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            cmd.CommandText = @"INSERT INTO Oferta (DizimistaID, Valor, Data, Observacao) VALUES (@dizimistaId,@valor,@data,@observacao); SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"UPDATE Oferta SET DizimistaID=@dizimistaId, Valor=@valor, Data=@data, Observacao=@observacao WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", item.ID);
        }
        cmd.Parameters.AddWithValue("@dizimistaId", item.DizimistaID);
        cmd.Parameters.AddWithValue("@valor", item.Valor);
        cmd.Parameters.AddWithValue("@data", item.Data.ToString("o"));
        cmd.Parameters.AddWithValue("@observacao", item.Observacao ?? string.Empty);
        var result = await cmd.ExecuteScalarAsync();
        if (item.ID == 0) item.ID = Convert.ToInt32(result);
        return item.ID;
    }

    public async Task<List<Oferta>> SearchByDizimistaIdAsync(int dizimistaId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Oferta WHERE DizimistaID = @dizimistaId ORDER BY Data DESC";
        cmd.Parameters.AddWithValue("@dizimistaId", dizimistaId);
        var list = new List<Oferta>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Oferta { ID = reader.GetInt32(0), DizimistaID = reader.GetInt32(1), Valor = reader.GetDecimal(2), Data = DateTime.Parse(reader.GetString(3)), Observacao = reader.IsDBNull(4) ? string.Empty : reader.GetString(4) });
        }
        return list;
    }
}
