using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Restringida;

/// <summary>Migración de MadisaNetR2/Restringida/Tickets_Caja.asp (facturas simplificadas por terminal).</summary>
public class TicketsCajaModel(Db db) : PageModel
{
    public string V1 = "";
    public string V2 = "";
    public string V3 = "";

    public List<Ticket> Tickets { get; } = new();

    public class Ticket
    {
        public IDictionary<string, object>? Cabecera;
        public List<IDictionary<string, object>> Filas { get; } = new();
        public decimal TotalSinDto;
        public decimal TotalTicket;
    }

    public async Task OnGetAsync(string? v1, string? v2, string? v3)
    {
        V1 = Fmt.FechaParam(v1);
        V2 = string.IsNullOrEmpty(v2) ? "50" : v2;
        V3 = string.IsNullOrEmpty(v3) ? "1" : v3;

        var filas = await db.Sp("MadisaNet", "rep.MovTicket_Term",
            new { Alm = V2, Fecha = V1, TermNo = V3 });

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
