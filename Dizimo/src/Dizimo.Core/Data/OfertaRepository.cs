using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dizimo.Core.Models;

namespace Dizimo.Core.Data;

public class OfertaRepository
{
    private readonly string _dbPath;

    public OfertaRepository(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitAsync()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Ofertas (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            DizimistaID INTEGER NOT NULL,
            Valor REAL NOT NULL,
            Data TEXT NOT NULL,
            Observacao TEXT
        );";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> SaveAsync(Oferta o)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        if (o.ID == 0)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Ofertas (DizimistaID, Valor, Data, Observacao)
                                VALUES ($did, $valor, $data, $obs);
                                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$did", o.DizimistaID);
            cmd.Parameters.AddWithValue("$valor", o.Valor);
            cmd.Parameters.AddWithValue("$data", o.Data.ToString("o"));
            cmd.Parameters.AddWithValue("$obs", o.Observacao ?? string.Empty);
            var res = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(res);
        }
        else
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Ofertas SET DizimistaID=$did, Valor=$valor, Data=$data, Observacao=$obs WHERE ID=$id";
            cmd.Parameters.AddWithValue("$did", o.DizimistaID);
            cmd.Parameters.AddWithValue("$valor", o.Valor);
            cmd.Parameters.AddWithValue("$data", o.Data.ToString("o"));
            cmd.Parameters.AddWithValue("$obs", o.Observacao ?? string.Empty);
            cmd.Parameters.AddWithValue("$id", o.ID);
            await cmd.ExecuteNonQueryAsync();
            return o.ID;
        }
    }

    public async Task<List<Oferta>> ListAsync()
    {
        var list = new List<Oferta>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, DizimistaID, Valor, Data, Observacao FROM Ofertas ORDER BY Data DESC";
        using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var o = new Oferta
            {
                ID = rdr.GetInt32(0),
                DizimistaID = rdr.GetInt32(1),
                Valor = Convert.ToDecimal(rdr.GetDouble(2)),
                Data = DateTime.Parse(rdr.GetString(3)),
                Observacao = rdr.IsDBNull(4) ? null : rdr.GetString(4)
            };
            list.Add(o);
        }
        return list;
    }

    public async Task<List<Oferta>> ListByDateAsync(DateTime date)
    {
        var list = new List<Oferta>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        var start = date.Date;
        var end = start.AddDays(1);
        cmd.CommandText = "SELECT ID, DizimistaID, Valor, Data, Observacao FROM Ofertas WHERE Data >= $start AND Data < $end ORDER BY Data DESC";
        cmd.Parameters.AddWithValue("$start", start.ToString("o"));
        cmd.Parameters.AddWithValue("$end", end.ToString("o"));
        using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var o = new Oferta
            {
                ID = rdr.GetInt32(0),
                DizimistaID = rdr.GetInt32(1),
                Valor = Convert.ToDecimal(rdr.GetDouble(2)),
                Data = DateTime.Parse(rdr.GetString(3)),
                Observacao = rdr.IsDBNull(4) ? null : rdr.GetString(4)
            };
            list.Add(o);
        }
        return list;
    }

    public async Task<List<Oferta>> SearchByDizimistaIdAsync(int dizimistaId)
    {
        var list = new List<Oferta>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, DizimistaID, Valor, Data, Observacao FROM Ofertas WHERE DizimistaID=$did ORDER BY Data DESC";
        cmd.Parameters.AddWithValue("$did", dizimistaId);
        using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var o = new Oferta
            {
                ID = rdr.GetInt32(0),
                DizimistaID = rdr.GetInt32(1),
                Valor = Convert.ToDecimal(rdr.GetDouble(2)),
                Data = DateTime.Parse(rdr.GetString(3)),
                Observacao = rdr.IsDBNull(4) ? null : rdr.GetString(4)
            };
            list.Add(o);
        }
        return list;
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Ofertas WHERE ID=$id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
