using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Restringida;

/// <summary>Migración de MadisaNetR2/Restringida/TarjetasCredito.asp.</summary>
public class TarjetasCreditoModel(Db db) : PageModel
{
    public string V1 = "";
    public string V2 = "";
    public List<dynamic> Detalle { get; private set; } = new();
    public List<dynamic> PorOperacion { get; private set; } = new();

    public async Task OnGetAsync(string? v1, string? v2)
    {
        V1 = Fmt.FechaParam(v1);
        V2 = string.IsNullOrEmpty(v2) ? "19" : v2;

        Detalle      = await db.Sp("MadisaNet", "rep.CardPos_FechaAlm_List", new { Fecha = V1, Alm = V2 });
        PorOperacion = await db.Sp("MadisaNet", "rep.CardPos_FechaAlm_Oper", new { Alm = V2, Fecha = V1 });
    }
}
