using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/LogErrorPda.asp.</summary>
public class LogErrorPdaModel(Db db) : PageModel
{
    public string? P1;
    public string P2 = "";
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync(string? p1, string? p2, string? formato)
    {
        if (!string.IsNullOrEmpty(formato))
            Response.Headers.ContentDisposition = $"attachment; filename=Informe.{(formato == "Excel" ? "xls" : "doc")}";
        P1 = string.IsNullOrEmpty(p1) ? null : p1;
        P2 = p2 ?? "";
        Datos = await db.Sp("NavR2", "asp.ErrorLogPda", new { Equipo = P2, Fecha = P1 });
    }
}
