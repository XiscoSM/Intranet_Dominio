using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/MadisaCargaVentas.asp.</summary>
public class CargaVentasModel(Db db) : PageModel
{
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync(string? p1, string? formato)
    {
        if (!string.IsNullOrEmpty(formato))
            Response.Headers.ContentDisposition = $"attachment; filename=Informe.{(formato == "Excel" ? "xls" : "doc")}";
        Datos = await db.Sp("MadisaNet", "rep.CargaVentas_Select", new { Errores = string.IsNullOrEmpty(p1) ? "0" : p1 });
    }
}
