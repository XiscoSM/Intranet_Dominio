using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/MadisaControl.asp (frameset Sinc + Transacción + Carga Ventas).</summary>
public class MadisaControlModel(Db db) : PageModel
{
    public List<dynamic> TransRecibidas { get; private set; } = new();
    public List<dynamic> Alertas { get; private set; } = new();
    public List<dynamic> CargaVentas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        TransRecibidas = await db.Sp("MadisaNet", "rep.TransRecibidas");
        Alertas        = await db.Sp("MadisaNet", "rep.Alertas");
        CargaVentas    = await db.Sp("MadisaNet", "rep.CargaVentas_Select", new { Errores = "0" });
    }
}
