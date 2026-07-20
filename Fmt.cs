using System.Globalization;

namespace Intranet;

/// <summary>Helpers de formato equivalentes a las funciones VBScript del ASP clásico.</summary>
public static class Fmt
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    /// <summary>Equivalente a FormatNumber(v, 2, -2, -2, -2).</summary>
    public static string N2(object? v) => D(v).ToString("N2", Es);

    /// <summary>Conversión defensiva a decimal (CDbl del VBScript). Null/DBNull/no numérico → 0.</summary>
    public static decimal D(object? v) => v switch
    {
        null or DBNull => 0m,
        decimal d => d,
        double d => (decimal)d,
        float f => (decimal)f,
        int i => i,
        long l => l,
        short s => s,
        byte b => b,
        string s when decimal.TryParse(s, NumberStyles.Any, Es, out var r) => r,
        string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) => r,
        _ => 0m
    };

    /// <summary>Texto plano; null → cadena vacía.</summary>
    public static string S(object? v) => v is null or DBNull ? "" : v.ToString() ?? "";

    /// <summary>Equivalente a DoDateTime(str, 3/4, ...) — hora HH:mm:ss.</summary>
    public static string Hora(object? v) => v switch
    {
        DateTime dt => dt.ToString("HH:mm:ss"),
        TimeSpan ts => ts.ToString(@"hh\:mm\:ss"),
        null or DBNull => "",
        _ => S(v)
    };

    /// <summary>Fecha corta dd/MM/yyyy.</summary>
    public static string Fecha(object? v) => v is DateTime dt ? dt.ToString("dd/MM/yyyy") : S(v);

    /// <summary>
    /// Normaliza el parámetro de fecha de las páginas al formato dd/MM/yy que esperan
    /// los stored procedures. Acepta vacío (hoy), yyyy-MM-dd (input type=date),
    /// dd/MM/yy y dd/MM/yyyy; quita comillas heredadas de los enlaces del ASP clásico.
    /// </summary>
    public static string FechaParam(string? v)
    {
        v = v?.Trim().Trim('\'');
        if (string.IsNullOrEmpty(v)) return DateTime.Today.ToString("dd/MM/yy");
        string[] formatos = ["yyyy-MM-dd", "dd/MM/yy", "dd/MM/yyyy"];
        if (DateTime.TryParseExact(v, formatos, Es, DateTimeStyles.None, out var dt))
            return dt.ToString("dd/MM/yy");
        return v;
    }

    /// <summary>Convierte el valor dd/MM/yy al formato yyyy-MM-dd de un input type=date.</summary>
    public static string FechaInput(string? v)
    {
        string[] formatos = ["dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd"];
        return DateTime.TryParseExact(v?.Trim(), formatos, Es, DateTimeStyles.None, out var dt)
            ? dt.ToString("yyyy-MM-dd")
            : DateTime.Today.ToString("yyyy-MM-dd");
    }

    /// <summary>Equivalente a la función formatear() de Tickets.asp: rellena con "_" hasta la longitud dada.</summary>
    public static string Pad(object? v, int caracteres, bool izquierda)
    {
        var t = S(v);
        return izquierda ? t.PadLeft(caracteres, '_') : t.PadRight(caracteres, '_');
    }
}
