using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/Etiquetas.asp.</summary>
public class EtiquetasModel(Db db) : PageModel
{
    public string? P1;
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync(string? p1, string? formato)
    {
        if (!string.IsNullOrEmpty(formato))
            Response.Headers.ContentDisposition = $"attachment; filename=Informe.{(formato == "Excel" ? "xls" : "doc")}";
        P1 = string.IsNullOrEmpty(p1) ? null : p1;
        Datos = await db.Sp("NavR2", "rep.InfoEtiquetas", new { Fecha = P1 });
    }
}
