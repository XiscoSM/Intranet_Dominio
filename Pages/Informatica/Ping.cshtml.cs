using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Informatica;

/// <summary>Migración de Informatica/Ping.asp: muestra los .txt que genera el job de ping.</summary>
public class PingModel(IConfiguration cfg) : PageModel
{
    public List<(string Titulo, string Contenido)> Paneles { get; } = new();

    private static readonly string[] Ficheros =
        ["PingCriticos.txt", "PingCriticoshist.txt", "PingNoCriticos.txt", "PingNoApagados.txt"];

    public void OnGet()
    {
        var carpeta = cfg["Ping:Carpeta"] ?? "";
        foreach (var f in Ficheros)
        {
            var ruta = Path.Combine(carpeta, f);
            var contenido = System.IO.File.Exists(ruta)
                ? System.IO.File.ReadAllText(ruta)
                : $"(no existe {ruta})";
            Paneles.Add((Path.GetFileNameWithoutExtension(f), contenido));
        }
    }
}
