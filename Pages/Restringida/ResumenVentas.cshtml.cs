using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Restringida;

/// <summary>Migración de MadisaNetR2/Restringida/ResumenVentas.asp.</summary>
public class ResumenVentasModel(Db db) : PageModel
{
    public string V1 = "";
    public string V2 = "";
    public List<dynamic> Ventas { get; private set; } = new();
    public decimal Total, TotalProductos;
    public decimal NumTickets;

    public async Task OnGetAsync(string? v1, string? v2)
    {
        V1 = Fmt.FechaParam(v1);
        V2 = string.IsNullOrEmpty(v2) ? "215" : v2;

        Ventas = await db.Sp("MadisaNet", "rep.VentasProg", new { Alm = V2, Fecha = V1 });
        var tickets = await db.Sp("MadisaNet", "rep.Ventas_NumTickets", new { Alm = V2, Fecha = V1 });
        NumTickets = tickets.Count > 0 ? Fmt.D(tickets[0].Ticks) : 0;

        foreach (var r in Ventas)
        {
            Total += Fmt.D(r.Importe);
            TotalProductos += Fmt.D(r.Prods);
        }
    }
}
