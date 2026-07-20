using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/MadisaTransaccion.asp.</summary>
public class AlertasModel(Db db) : PageModel
{
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync(string? formato)
    {
        if (!string.IsNullOrEmpty(formato))
            Response.Headers.ContentDisposition = $"attachment; filename=Informe.{(formato == "Excel" ? "xls" : "doc")}";
        Datos = await db.Sp("MadisaNet", "rep.Alertas");
    }
}
