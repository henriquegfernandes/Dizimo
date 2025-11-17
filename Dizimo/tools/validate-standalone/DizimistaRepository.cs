using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DizimistaRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public DizimistaRepository(ILogger logger)
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
            CREATE TABLE IF NOT EXISTS Dizimista (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Codigo TEXT NOT NULL,
                DataNascimento TEXT,
                Ativo INTEGER NOT NULL,
                Bloqueado INTEGER NOT NULL
            );";
        await createTableCmd.ExecuteNonQueryAsync();
        _hasBeenInitialized = true;
    }

    public async Task<int> SaveAsync(Dizimista item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            cmd.CommandText = @"INSERT INTO Dizimista (Nome, Codigo, DataNascimento, Ativo, Bloqueado) VALUES (@nome,@codigo,@data,@ativo,@bloqueado); SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"UPDATE Dizimista SET Nome=@nome, Codigo=@codigo, DataNascimento=@data, Ativo=@ativo, Bloqueado=@bloqueado WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", item.ID);
        }
        cmd.Parameters.AddWithValue("@nome", item.Nome);
        cmd.Parameters.AddWithValue("@codigo", item.Codigo);
        cmd.Parameters.AddWithValue("@data", item.DataNascimento?.ToString("o") ?? string.Empty);
        cmd.Parameters.AddWithValue("@ativo", item.Ativo ? 1 : 0);
        cmd.Parameters.AddWithValue("@bloqueado", item.Bloqueado ? 1 : 0);
        var result = await cmd.ExecuteScalarAsync();
        if (item.ID == 0) item.ID = Convert.ToInt32(result);
        return item.ID;
    }

    public async Task<List<Dizimista>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Dizimista ORDER BY Nome COLLATE NOCASE";
        var list = new List<Dizimista>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Dizimista
            {
                ID = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Codigo = reader.GetString(2),
                DataNascimento = string.IsNullOrEmpty(reader.GetString(3)) ? null : DateTime.Parse(reader.GetString(3)),
                Ativo = reader.GetInt32(4) == 1,
                Bloqueado = reader.GetInt32(5) == 1
            });
        }
        return list;
    }

    public async Task<Dizimista?> GetByIdAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Dizimista WHERE ID = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return new Dizimista { ID = reader.GetInt32(0), Nome = reader.GetString(1), Codigo = reader.GetString(2), DataNascimento = string.IsNullOrEmpty(reader.GetString(3)) ? null : DateTime.Parse(reader.GetString(3)), Ativo = reader.GetInt32(4) == 1, Bloqueado = reader.GetInt32(5) == 1 };
        return null;
    }

    public async Task<Dizimista?> GetByCodigoAsync(string codigo)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Dizimista WHERE Codigo = @codigo";
        cmd.Parameters.AddWithValue("@codigo", codigo);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return new Dizimista { ID = reader.GetInt32(0), Nome = reader.GetString(1), Codigo = reader.GetString(2), DataNascimento = string.IsNullOrEmpty(reader.GetString(3)) ? null : DateTime.Parse(reader.GetString(3)), Ativo = reader.GetInt32(4) == 1, Bloqueado = reader.GetInt32(5) == 1 };
        return null;
    }
}
