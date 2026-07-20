using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/ControlGeneral.asp (frameset de 5 paneles).</summary>
public class ControlGeneralModel(Db db, IConfiguration cfg) : PageModel
{
    public string CajasFueraLinea = "0";
    public List<dynamic> Alertas { get; private set; } = new();
    public List<dynamic> HorarioSinc { get; private set; } = new();
    public List<dynamic> LogErrorPda { get; private set; } = new();
    public List<(string Titulo, string Contenido)> Ping { get; } = new();

    private static readonly string[] FicherosPing =
        ["PingCriticos.txt", "PingCriticoshist.txt", "PingNoCriticos.txt", "PingNoApagados.txt"];

    public async Task OnGetAsync()
    {
        var fuera = await db.Sp("MadisaNet", "rep.CajasFueraLineaR2");
        if (fuera.Count > 0) CajasFueraLinea = Fmt.S(((IDictionary<string, object>)fuera[0])["Error"]);

        Alertas     = await db.Sp("MadisaNet", "rep.Alertas");
        HorarioSinc = await db.Sp("NavR2", "asp.Info_SincControlHorario");
        LogErrorPda = await db.Sp("NavR2", "asp.ErrorLogPda", new { Equipo = "", Fecha = (string?)null });

        var carpeta = cfg["Ping:Carpeta"] ?? "";
        foreach (var f in FicherosPing)
        {
            var ruta = Path.Combine(carpeta, f);
            Ping.Add((Path.GetFileNameWithoutExtension(f),
                System.IO.File.Exists(ruta) ? System.IO.File.ReadAllText(ruta) : ""));
        }
    }
}
