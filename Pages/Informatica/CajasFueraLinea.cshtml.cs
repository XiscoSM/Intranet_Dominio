using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/MadisaCajasFueraLinea.asp.</summary>
public class CajasFueraLineaModel(Db db) : PageModel
{
    public string Numero = "0";

    public async Task OnGetAsync()
    {
        var datos = await db.Sp("MadisaNet", "rep.CajasFueraLineaR2");
        if (datos.Count > 0) Numero = Fmt.S(((IDictionary<string, object>)datos[0])["Error"]);
    }
}
