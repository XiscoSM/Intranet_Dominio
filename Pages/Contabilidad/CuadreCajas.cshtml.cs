using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Contabilidad;

/// <summary>Migración de MadisaNetR2/Contabilidad/CuadreCajasList.asp.</summary>
public class CuadreCajasModel(Db db) : PageModel
{
    public string V1 = "";
    public string V2 = "";
    public bool Support;

    public List<Linea> Lineas { get; } = new();
    public Linea Totales { get; } = new() { Tienda = "", Operador = "TOTAL" };
    public List<dynamic> Tarjetas { get; private set; } = new();
    public List<dynamic> TarjetasTef { get; private set; } = new();
    public List<dynamic> Resumen { get; private set; } = new();

    /// <summary>Línea agrupada por (Tienda, Operador) — réplica de los acumuladores VBScript.</summary>
    public class Linea
    {
        public string Tienda = "", Operador = "", Nombre = "";
        public decimal Et, Ed, Entrada, Ret, Pg;          // efectivo, entrada, retirada, pago
        public decimal D1t, D1d;                          // datáfono Visa
        public decimal D2t, D2d;                          // datáfono otros
        public decimal Tt, Td;                            // TEF
        public decimal Jt, Jd;                            // justificantes
        public decimal Ct, Cd;                            // crédito (tipo 5)
        public decimal Gt, Gd;                            // cupones
        public decimal Pgrt, Pgrd;                        // PayGold
        public string TextoTef = "", TextoDat = "", TextoPago = "", TextoCupon = "";

        public decimal TotalTeorico => Et + D1t + D2t + Tt + Jt + Gt + Ct + Pg + Pgrt;
        public decimal TotalDeclarado => Ed + D1d + D2d + Td + Jd + Gd + Cd + Ret - Entrada + Pg + Pgrd;
        public decimal Diferencia => TotalDeclarado - TotalTeorico;
    }

    public async Task OnGetAsync(string? v1, string? v2, string? support)
    {
        V1 = Fmt.FechaParam(v1);
        V2 = string.IsNullOrEmpty(v2) ? "215" : v2;
        Support = !string.IsNullOrEmpty(support);

        var filas    = await db.Sp("MadisaNet", "rep.CuadreCajas",             new { Fecha = V1, Alm = V2 });
        Tarjetas     = await db.Sp("MadisaNet", "rep.CuadreCajas_Tarjetas",    new { Fecha = V1, Alm = V2 });
        TarjetasTef  = await db.Sp("MadisaNet", "rep.CardPos_FechaAlm",        new { Alm = V2, Fecha = V1 });
        Resumen      = await db.Sp("MadisaNet", "rep.CuadreCajas_ResumCajero", new { Alm = V2, Fecha = V1 });

        Linea? l = null;
        foreach (IDictionary<string, object> r in filas)
        {
            var oper = Fmt.S(r["OperNo"]);
            var tienda = Fmt.S(r["StoreNo"]);
            if (l is null || l.Operador != oper || l.Tienda != tienda)
            {
                Cerrar(l);
                l = new Linea { Operador = oper, Tienda = tienda };
            }
            Aplicar(l, r);
        }
        Cerrar(l);
    }

    private void Cerrar(Linea? l)
    {
        if (l is null) return;
        Lineas.Add(l);
        Totales.Et += l.Et; Totales.Ed += l.Ed;
        Totales.Entrada += l.Entrada; Totales.Ret += l.Ret; Totales.Pg += l.Pg;
        Totales.D1t += l.D1t; Totales.D1d += l.D1d;
        Totales.D2t += l.D2t; Totales.D2d += l.D2d;
        Totales.Tt += l.Tt; Totales.Td += l.Td;
        Totales.Jt += l.Jt; Totales.Jd += l.Jd;
        Totales.Ct += l.Ct; Totales.Cd += l.Cd;
        Totales.Gt += l.Gt; Totales.Gd += l.Gd;
        Totales.Pgrt += l.Pgrt; Totales.Pgrd += l.Pgrd;
    }

    private void Aplicar(Linea l, IDictionary<string, object> r)
    {
        var tipo = (int)Fmt.D(r["PayNo"]);
        var importe = Fmt.D(r["Importe"]);
        var declarado = Fmt.D(r["Declarado"]);
        var pago = Fmt.D(r["Pago"]);

        l.Nombre = Fmt.S(r["DescOper"]);
        l.Pg += pago;
        if (tipo > 1 && pago > 0) l.TextoPago = "Pago distinto a Efectivo";

        switch (tipo)
        {
            case 1:
                l.Et = importe;
                if (!Support) l.Et -= Fmt.D(r["Retirada2"]); // quitamos del efectivo la retirada berna
                l.Ed = declarado;
                l.Entrada = Fmt.D(r["Entrada"]);
                break;
            case 2:
                l.D1t = importe; l.D1d = declarado;
                break;
            case 3 or 6 or 7 or 8:
                l.D2t += importe; l.D2d += declarado;
                l.TextoDat += $"Tipo {tipo} Teorico:{importe} Declarado:{declarado}\r";
                break;
            case 4 or 9 or 16:
                l.Tt += importe; l.Td += declarado;
                l.TextoTef += $"Tipo {tipo} Teorico:{importe} Declarado:{declarado}\r";
                break;
            case 5:
                l.Ct = importe; l.Cd = declarado;
                break;
            case 10 or 13:
                l.Jt += importe; l.Jd += declarado;
                break;
            case 15 or 11 or 17:
                l.Gt += importe; l.Gd += declarado;
                l.TextoCupon += $"Tipo {tipo} Teorico:{importe} Declarado:{declarado}\r";
                break;
            case 14:
                l.Pgrt = importe; l.Pgrd = declarado;
                break;
        }

        l.Ret = Fmt.D(r["Retirada"]);
        if (Support) l.Ret += Fmt.D(r["Retirada2"]);
    }
}
