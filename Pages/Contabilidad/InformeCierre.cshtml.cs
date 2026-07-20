using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Contabilidad;

/// <summary>Migración de MadisaNetR2/Contabilidad/Informe Cierre.asp.</summary>
public class InformeCierreModel(Db db) : PageModel
{
    public string P2 = "";
    public string Almacen = "";
    public List<dynamic> Seccion { get; private set; } = new();
    public List<dynamic> TefFirma { get; private set; } = new();
    public List<dynamic> CambiosPvp { get; private set; } = new();

    public async Task OnGetAsync(string? p2, string? v1, string? v2)
    {
        var empresa = v1 ?? Request.Cookies["EMPRESA"] ?? "HM1";
        Almacen = v2 ?? Request.Cookies["ALMACEN"] ?? "0";
        Response.Cookies.Append("EMPRESA", empresa);
        Response.Cookies.Append("ALMACEN", Almacen);

        P2 = Fmt.FechaParam(p2);

        Seccion    = await db.Sp("MadisaNet", "rep.Seccion",          new { Alm = Almacen, Fecha = P2 });
        TefFirma   = await db.Sp("MadisaNet", "rep.TeffFirma",        new { Alm = Almacen, Fecha = P2 });
        CambiosPvp = await db.Sp("MadisaNet", "rep.CambiosPvp_Varios", new { Alm = Almacen, Fecha = P2 });
    }
}
