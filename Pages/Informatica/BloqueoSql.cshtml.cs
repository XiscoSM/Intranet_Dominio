using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/BloqueoSql.asp.</summary>
public class BloqueoSqlModel(Db db) : PageModel
{
    public List<dynamic> Datos { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Datos = await db.Sp("NavR2", "asp.BloqueosSql");
    }
}
