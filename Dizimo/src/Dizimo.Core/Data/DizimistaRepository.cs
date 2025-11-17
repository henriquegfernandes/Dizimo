using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dizimo.Core.Models;

namespace Dizimo.Core.Data;

public class DizimistaRepository
{
    private readonly string _dbPath;

    public DizimistaRepository(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitAsync()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Dizimistas (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL,
            Codigo TEXT NOT NULL,
            DataNascimento TEXT,
            Ativo INTEGER,
            Bloqueado INTEGER
        );";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> SaveAsync(Dizimista d)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        if (d.ID == 0)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Dizimistas (Nome, Codigo, DataNascimento, Ativo, Bloqueado)
                                VALUES ($nome, $codigo, $data, $ativo, $bloqueado);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$nome", d.Nome);
            cmd.Parameters.AddWithValue("$codigo", d.Codigo);
            cmd.Parameters.AddWithValue("$data", d.DataNascimento.HasValue ? (object)d.DataNascimento.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("$ativo", d.Ativo ? 1 : 0);
            cmd.Parameters.AddWithValue("$bloqueado", d.Bloqueado ? 1 : 0);
            var res = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(res);
        }
        else
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Dizimistas SET Nome=$nome, Codigo=$codigo, DataNascimento=$data, Ativo=$ativo, Bloqueado=$bloqueado WHERE ID=$id";
            cmd.Parameters.AddWithValue("$nome", d.Nome);
            cmd.Parameters.AddWithValue("$codigo", d.Codigo);
            cmd.Parameters.AddWithValue("$data", d.DataNascimento.HasValue ? (object)d.DataNascimento.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("$ativo", d.Ativo ? 1 : 0);
            cmd.Parameters.AddWithValue("$bloqueado", d.Bloqueado ? 1 : 0);
            cmd.Parameters.AddWithValue("$id", d.ID);
            await cmd.ExecuteNonQueryAsync();
            return d.ID;
        }
    }

    public async Task<List<Dizimista>> ListAsync()
    {
        var list = new List<Dizimista>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Nome, Codigo, DataNascimento, Ativo, Bloqueado FROM Dizimistas ORDER BY Nome";
        using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var d = new Dizimista
            {
                ID = rdr.GetInt32(0),
                Nome = rdr.GetString(1),
                Codigo = rdr.GetString(2),
                DataNascimento = rdr.IsDBNull(3) ? null : DateTime.Parse(rdr.GetString(3)),
                Ativo = rdr.GetInt32(4) == 1,
                Bloqueado = rdr.GetInt32(5) == 1
            };
            list.Add(d);
        }
        return list;
    }

    public async Task<Dizimista?> GetByIdAsync(int id)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Nome, Codigo, DataNascimento, Ativo, Bloqueado FROM Dizimistas WHERE ID=$id";
        cmd.Parameters.AddWithValue("$id", id);
        using var rdr = await cmd.ExecuteReaderAsync();
        if (await rdr.ReadAsync())
        {
            return new Dizimista
            {
                ID = rdr.GetInt32(0),
                Nome = rdr.GetString(1),
                Codigo = rdr.GetString(2),
                DataNascimento = rdr.IsDBNull(3) ? null : DateTime.Parse(rdr.GetString(3)),
                Ativo = rdr.GetInt32(4) == 1,
                Bloqueado = rdr.GetInt32(5) == 1
            };
        }
        return null;
    }

    public async Task<Dizimista?> GetByCodigoAsync(string codigo)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Nome, Codigo, DataNascimento, Ativo, Bloqueado FROM Dizimistas WHERE Codigo=$codigo";
        cmd.Parameters.AddWithValue("$codigo", codigo);
        using var rdr = await cmd.ExecuteReaderAsync();
        if (await rdr.ReadAsync())
        {
            return new Dizimista
            {
                ID = rdr.GetInt32(0),
                Nome = rdr.GetString(1),
                Codigo = rdr.GetString(2),
                DataNascimento = rdr.IsDBNull(3) ? null : DateTime.Parse(rdr.GetString(3)),
                Ativo = rdr.GetInt32(4) == 1,
                Bloqueado = rdr.GetInt32(5) == 1
            };
        }
        return null;
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Dizimistas WHERE ID=$id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
