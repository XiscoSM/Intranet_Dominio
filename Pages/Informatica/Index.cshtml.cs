using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/inicio.asp (portal de informática con selector de almacén).</summary>
public class IndexModel(Db db) : PageModel
{
    public string Empresa = "HM1";
    public string Almacen = "1";
    public string DescAlm = "";
    public List<dynamic> Almacenes { get; private set; } = new();

    public async Task OnGetAsync(string? v1, string? v2)
    {
        // Misma lógica de cookies EMPRESA/ALMACEN que el ASP original
        Empresa = v1 ?? Request.Cookies["EMPRESA"] ?? "HM1";
        Almacen = v2 ?? Request.Cookies["ALMACEN"] ?? "1";
        Response.Cookies.Append("EMPRESA", Empresa);
        Response.Cookies.Append("ALMACEN", Almacen);

        Almacenes = await db.Sp("NavR2", "asp.Almacen", new { Empresa, Alm = -1 });

        var actual = await db.Sp("NavR2", "asp.Almacen", new { Empresa, Alm = Almacen });
        if (actual.Count > 0) DescAlm = Fmt.S(((IDictionary<string, object>)actual[0])["DescAlm"]);
    }
}
