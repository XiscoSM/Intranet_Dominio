using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Intranet.Data;

/// <summary>
/// Acceso a datos vía Dapper. Sustituye a los ADODB.Recordset sobre DSN ODBC del ASP clásico.
/// Las fechas se pasan como string (dd/MM/yy) igual que hacía el ASP original, y SQL Server
/// las convierte al tipo del parámetro del procedimiento.
/// </summary>
public sealed class Db(IConfiguration cfg)
{
    // Modo demo (sin SQL Server): devuelve datos de ejemplo. Solo appsettings.Development.json.
    private readonly bool _demo = cfg.GetValue<bool>("BaseDeDatos:Demo");

    private SqlConnection Open(string name) =>
        new(cfg.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Falta ConnectionStrings:{name} en appsettings.json"));

    /// <summary>Ejecuta un stored procedure y devuelve las filas como objetos dinámicos.</summary>
    public async Task<List<dynamic>> Sp(string conexion, string procedimiento, object? parametros = null)
    {
        if (_demo) return DemoData.Get(procedimiento);

        await using var c = Open(conexion);
        var filas = await c.QueryAsync(procedimiento, parametros, commandType: CommandType.StoredProcedure);
        return filas.ToList();
    }
}
