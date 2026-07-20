using Intranet.Data;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Autenticación Windows (usuario del dominio, igual que el ASP clásico con Windows Auth en IIS)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

// Grupos AD configurables en appsettings.json (Autorizacion:Grupos) — sustituyen a las ACL NTFS por carpeta
var grupos = builder.Configuration.GetSection("Autorizacion:Grupos");
string Grupo(string clave) => grupos[clave]
    ?? throw new InvalidOperationException($"Falta Autorizacion:Grupos:{clave} en appsettings.json");

// Modo desarrollo fuera del dominio: las policies solo exigen usuario autenticado (Windows local),
// sin comprobar grupos AD. Activado únicamente en appsettings.Development.json — nunca en producción.
var sinGrupos = builder.Configuration.GetValue<bool>("Autorizacion:OmitirGruposAD");

builder.Services.AddAuthorization(options =>
{
    // Todo requiere usuario autenticado del dominio
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    if (sinGrupos)
    {
        var soloAutenticado = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        options.AddPolicy("Informatica", soloAutenticado);
        options.AddPolicy("Contabilidad", soloAutenticado);
        options.AddPolicy("Restringida", soloAutenticado);
    }
    else
    {
        options.AddPolicy("Informatica", p => p.RequireRole(Grupo("Informatica")));
        options.AddPolicy("Contabilidad", p => p.RequireRole(Grupo("Contabilidad")));
        options.AddPolicy("Restringida", p => p.RequireRole(Grupo("Restringida")));
    }
});

builder.Services.AddRazorPages(options =>
{
    // Equivalencia 1:1 con los permisos NTFS de las carpetas originales
    options.Conventions.AuthorizeFolder("/Informatica", "Informatica");
    options.Conventions.AuthorizeFolder("/Contabilidad", "Contabilidad");
    options.Conventions.AuthorizeFolder("/Restringida", "Restringida");
});

builder.Services.AddSingleton<Db>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
