using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/HorarioSinc.asp.</summary>
public class HorarioSincModel(Db db) : PageModel
{
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync(string? formato)
    {
        if (!string.IsNullOrEmpty(formato))
            Response.Headers.ContentDisposition = $"attachment; filename=Informe.{(formato == "Excel" ? "xls" : "doc")}";
        Datos = await db.Sp("NavR2", "asp.Info_SincControlHorario");
    }
}
