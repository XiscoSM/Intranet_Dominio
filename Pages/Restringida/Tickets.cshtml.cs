using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Restringida;

/// <summary>Migración de MadisaNetR2/Restringida/Tickets.asp (detalle de tickets por cajero).</summary>
public class TicketsModel(Db db) : PageModel
{
    public string V1 = "";
    public string V2 = "";
    public string V3 = "";
    public string? V4;

    public List<Ticket> Tickets { get; } = new();

    public class Ticket
    {
        public IDictionary<string, object>? Cabecera;
        public List<IDictionary<string, object>> Filas { get; } = new();
        public decimal TotalSinDto;   // suma de Amount de los productos
        public decimal TotalTicket;   // ImporteTicket de la cabecera
    }

    public async Task OnGetAsync(string? v1, string? v2, string? v3, string? v4)
    {
        V1 = Fmt.FechaParam(v1);
        V2 = string.IsNullOrEmpty(v2) ? "50" : v2;
        V3 = string.IsNullOrEmpty(v3) ? "1" : v3;
        V4 = string.IsNullOrEmpty(v4) || v4.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : v4;

        var filas = await db.Sp("MadisaNet", "rep.MovTicket",
            new { Alm = V2, Fecha = V1, OperNo = V3, TransType = V4 });

        // Agrupación secuencial por Counter, como el bucle VBScript original
        Ticket? t = null;
        object? counter = null;
        foreach (IDictionary<string, object> r in filas)
        {
            if (t is null || !Equals(Fmt.S(r["Counter"]), Fmt.S(counter)))
            {
                t = new Ticket();
                Tickets.Add(t);
                counter = r["Counter"];
            }
            var tipoReg = Fmt.S(r["TipoRegistro"]);
            if (tipoReg == "0CabeceraTicket")
            {
                t.Cabecera = r;
                t.TotalTicket = Fmt.D(r["ImporteTicket"]);
            }
            else
            {
                t.Filas.Add(r);
                if (tipoReg == "10producto") t.TotalSinDto += Fmt.D(r["Amount"]);
            }
        }
    }
}
