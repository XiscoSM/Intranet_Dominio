using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Contabilidad;

/// <summary>Migración de MadisaNetR2/Contabilidad/Informe Balanzas.asp.</summary>
public class InformeBalanzasModel(Db db) : PageModel
{
    public string P2 = "";
    public string Almacen = "";
    public List<dynamic> Balanzas { get; private set; } = new();

    public async Task OnGetAsync(string? p2, string? v1, string? v2)
    {
        // Misma lógica de cookies EMPRESA/ALMACEN que el ASP original
        var empresa = v1 ?? Request.Cookies["EMPRESA"] ?? "HM1";
        Almacen = v2 ?? Request.Cookies["ALMACEN"] ?? "0";
        Response.Cookies.Append("EMPRESA", empresa);
        Response.Cookies.Append("ALMACEN", Almacen);

        P2 = Fmt.FechaParam(p2);

        Balanzas = await db.Sp("MadisaNet", "rep.Balanzas", new { Alm = Almacen, Fecha = P2 });
    }
}
