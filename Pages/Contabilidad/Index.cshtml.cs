using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Contabilidad;

/// <summary>
/// Punto de entrada de Contabilidad — sustituye a CuadreCajasResum.asp
/// (no estaba en la copia del sitio): selector de fecha y tienda con
/// accesos directos a los informes de cada almacén.
/// </summary>
public class IndexModel(Db db) : PageModel
{
    public string Fecha = "";
    public List<dynamic> Almacenes { get; private set; } = new();

    public async Task OnGetAsync(string? v1)
    {
        Fecha = Fmt.FechaParam(v1);
        var empresa = Request.Cookies["EMPRESA"] ?? "HM1";
        Almacenes = await db.Sp("NavR2", "asp.Almacen", new { Empresa = empresa, Alm = -1 });
    }
}
