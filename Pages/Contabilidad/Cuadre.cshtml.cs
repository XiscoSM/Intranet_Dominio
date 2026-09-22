using System.Globalization;
using Intranet.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet.Pages.Contabilidad;

/// <summary>
/// Cash-up review redesigned around the accountant's task: "does the store balance?",
/// then "which cashier and why?", then "show me the tickets".
/// Same stored procedures and the same totals as CuadreCajas (the legacy port), but the
/// per-payment detail lives in an expandable row instead of 20 columns and tooltips.
/// </summary>
public class CuadreModel(Db db, IConfiguration cfg) : PageModel
{
    // Payment numbers the legacy report adds into the totals. Anything else is shown but not counted.
    private static readonly HashSet<int> CountedPayNos = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 17];
    private const int CashPayNo = 1;
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public DateTime Date { get; private set; }
    public string Store { get; private set; } = "";
    public string StoreName { get; private set; } = "";
    public bool Support { get; private set; }
    public bool OnlyIssues { get; private set; }

    public decimal Tolerance { get; private set; }
    public decimal MismatchThreshold { get; private set; }

    public List<StoreOption> Stores { get; } = new();
    public StoreOption? PreviousStore { get; private set; }
    public StoreOption? NextStore { get; private set; }

    public List<Cashier> Cashiers { get; } = new();
    public List<CardLine> Cards { get; } = new();
    public List<dynamic> TefOperations { get; private set; } = new();

    public decimal Theoretical => Cashiers.Sum(c => c.Theoretical);
    public decimal Declared => Cashiers.Sum(c => c.Declared);
    public decimal Difference => Declared - Theoretical;
    public decimal AbsoluteDifference => Cashiers.Sum(c => Math.Abs(c.Difference));
    public int CashiersWithIssues => Cashiers.Count(c => c.Status >= CashStatus.Review);
    public CashStatus StoreStatus => Classify(Difference);
    public decimal CardsDifference => Cards.Sum(c => c.Difference);

    /// <summary>Large opposite differences between cashiers that cancel out at store level.</summary>
    public bool CrossedDifferences =>
        Cashiers.Any(c => c.Difference >= MismatchThreshold) &&
        Cashiers.Any(c => c.Difference <= -MismatchThreshold) &&
        Math.Abs(Difference) < MismatchThreshold;

    public string DateParam => Date.ToString("yyyy-MM-dd");
    public string DateText => Date.ToString("dddd, d 'de' MMMM 'de' yyyy", Es);

    public enum CashStatus { Ok, Tolerance, Review, Mismatch }

    public sealed record StoreOption(string Code, string Name, int Cashiers)
    {
        public string Label => Name == "" ? Code : $"{Code} — {Name}";
    }

    public sealed class PaymentLine
    {
        public int PayNo;
        public string Name = "";
        public decimal Theoretical;
        public decimal Declared;
        public bool Counted;
        public bool IsCash;
        public decimal Difference => Declared - Theoretical;
    }

    /// <summary>One row of rep.CuadreCajas_ResumCajero: activity of a cashier on a till by ticket type.</summary>
    public sealed class Activity
    {
        public string Terminal = "";
        public string TicketType = "";   // "Venta", "Ticket Negativo", "Aparca ticket"...
        public string Schedule = "";     // "08:06 a 20:26", split out of TipoTicket
        public int Tickets;
        public decimal Amount;
        public int Alert;                // 1 = cashier is a supervisor, 2 = return without original ticket

        public bool ReturnWithoutOriginal => Alert == 2;
    }

    public sealed class CardLine
    {
        public string Type = "";
        public string Name = "";
        public decimal Theoretical;
        public decimal Declared;
        public decimal Difference => Declared - Theoretical;
    }

    public sealed class Cashier
    {
        public string Store = "";
        public string Code = "";
        public string Name = "";
        public List<PaymentLine> Lines { get; } = new();
        public List<Activity> Activity { get; } = new();

        public decimal CashCounted;   // cash declared by the cashier (raw Declarado of PayNo 1)
        public decimal Withdrawals;   // cash taken out of the till during the day
        public decimal OpeningFloat;  // "Entrada": change fund put into the till
        public decimal Payments;      // "Pago": added to both sides, never changes the difference

        public decimal Theoretical => Lines.Where(l => l.Counted).Sum(l => l.Theoretical) + Payments;
        public decimal Declared => Lines.Where(l => l.Counted).Sum(l => l.Declared) + Payments;
        public decimal Difference => Declared - Theoretical;
        public CashStatus Status { get; set; }

        public IEnumerable<string> Terminals =>
            Activity.Select(a => a.Terminal).Where(t => t != "").Distinct();
        public IEnumerable<Activity> ReturnsWithoutOriginal => Activity.Where(a => a.ReturnWithoutOriginal);
        public bool IsSupervisor => Activity.Any(a => a.Alert == 1) || Name.Contains("(Responsable)");
        public string DisplayName => Name.Replace("(Responsable)", "").Trim();
        public PaymentLine? CashLine => Lines.FirstOrDefault(l => l.IsCash);
        public IEnumerable<PaymentLine> VisibleLines =>
            Lines.Where(l => l.Theoretical != 0 || l.Declared != 0);
        public int HiddenLines => Lines.Count - VisibleLines.Count();

        /// <summary>Non-cash lines whose declared amount differs from the theoretical one.</summary>
        public IEnumerable<PaymentLine> NonCashIssues =>
            Lines.Where(l => !l.IsCash && l.Counted && l.Difference != 0);
    }

    public CashStatus Classify(decimal difference)
    {
        var abs = Math.Abs(difference);
        if (abs < 0.005m) return CashStatus.Ok;
        if (abs < Tolerance) return CashStatus.Tolerance;
        if (abs < MismatchThreshold) return CashStatus.Review;
        return CashStatus.Mismatch;
    }

    public async Task OnGetAsync(string? fecha, string? tienda, string? v1, string? v2, string? support, string? solo)
    {
        Tolerance = cfg.GetValue("Cuadre:Tolerancia", 1m);
        MismatchThreshold = cfg.GetValue("Cuadre:Descuadre", 20m);
        Support = !string.IsNullOrEmpty(support);
        OnlyIssues = solo == "1";

        // Default: yesterday, which is the day the accountants review each morning.
        var dateText = Fmt.FechaParam(fecha ?? v1 ?? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
        Date = DateTime.ParseExact(dateText, "dd/MM/yy", Es);

        var company = Request.Cookies["EMPRESA"] ?? "HM1";
        var activeTask = db.Sp("MadisaNet", "rep.CuadreCajas_Resum", new { Fecha = dateText });
        var namesTask = db.Sp("NavR2", "asp.Almacen", new { Empresa = company, Alm = -1 });
        await Task.WhenAll(activeTask, namesTask);

        var names = new Dictionary<string, string>();
        foreach (IDictionary<string, object> r in namesTask.Result)
            names[Fmt.S(r["Alm"])] = Fmt.S(r["DescAlm"]);

        foreach (IDictionary<string, object> r in activeTask.Result)
        {
            var code = Fmt.S(r["Alm"]);
            Stores.Add(new StoreOption(code, names.GetValueOrDefault(code, ""), (int)Fmt.D(r["Cajeros"])));
        }
        Stores.Sort((a, b) => Fmt.D(a.Code).CompareTo(Fmt.D(b.Code)));

        Store = tienda ?? v2 ?? Request.Cookies["ALMACEN"] ?? "";
        if (Store == "" || !int.TryParse(Store, out _))
            Store = Stores.FirstOrDefault()?.Code ?? "";
        if (Store == "") return;   // no activity at all on this date

        Response.Cookies.Append("ALMACEN", Store);
        StoreName = names.GetValueOrDefault(Store, "");
        if (Stores.All(s => s.Code != Store))
            Stores.Insert(0, new StoreOption(Store, StoreName, 0));   // keep the selected store visible
        var index = Stores.FindIndex(s => s.Code == Store);
        PreviousStore = index > 0 ? Stores[index - 1] : null;
        NextStore = index >= 0 && index < Stores.Count - 1 ? Stores[index + 1] : null;

        var args = new { Fecha = dateText, Alm = Store };
        var cashupTask = db.Sp("MadisaNet", "rep.CuadreCajas", args);
        var cardsTask = db.Sp("MadisaNet", "rep.CuadreCajas_Tarjetas", args);
        var tefTask = db.Sp("MadisaNet", "rep.CardPos_FechaAlm", args);
        var activityTask = db.Sp("MadisaNet", "rep.CuadreCajas_ResumCajero", args);
        await Task.WhenAll(cashupTask, cardsTask, tefTask, activityTask);

        BuildCashiers(cashupTask.Result);
        AttachActivity(activityTask.Result);

        foreach (IDictionary<string, object> r in cardsTask.Result)
            Cards.Add(new CardLine
            {
                Type = Fmt.S(r["Tipo"]),
                Name = Fmt.S(r["DescPago"]),
                Theoretical = Fmt.D(r["Teorico"]),
                Declared = Fmt.D(r["Declarado"]),
            });
        TefOperations = tefTask.Result;

        foreach (var c in Cashiers) c.Status = Classify(c.Difference);
        // Largest problems first: that is what the accountant is looking for.
        Cashiers.Sort((a, b) => Math.Abs(b.Difference).CompareTo(Math.Abs(a.Difference)));
    }

    private void BuildCashiers(List<dynamic> rows)
    {
        Cashier? current = null;
        foreach (IDictionary<string, object> r in rows)
        {
            var store = Fmt.S(r["StoreNo"]);
            var code = Fmt.S(r["OperNo"]);
            if (current is null || current.Code != code || current.Store != store)
            {
                current = Cashiers.FirstOrDefault(c => c.Code == code && c.Store == store);
                if (current is null)
                {
                    current = new Cashier { Store = store, Code = code };
                    Cashiers.Add(current);
                }
            }

            var payNo = (int)Fmt.D(r["PayNo"]);
            var theoretical = Fmt.D(r["Importe"]);
            var declared = Fmt.D(r["Declarado"]);
            var withdrawal2 = Fmt.D(r["Retirada2"]);

            current.Name = Fmt.S(r["DescOper"]);
            current.Payments += Fmt.D(r["Pago"]);
            // Withdrawals come repeated on every row of the cashier (same as the legacy report).
            current.Withdrawals = Fmt.D(r["Retirada"]) + (Support ? withdrawal2 : 0);

            if (payNo == CashPayNo)
            {
                current.CashCounted = declared;
                current.OpeningFloat = Fmt.D(r["Entrada"]);
                if (!Support) theoretical -= withdrawal2;
            }

            current.Lines.Add(new PaymentLine
            {
                PayNo = payNo,
                Name = r.TryGetValue("Forma de Pago", out var n) && Fmt.S(n) != "" ? Fmt.S(n) : $"Forma de pago {payNo}",
                Theoretical = theoretical,
                Declared = declared,
                Counted = CountedPayNos.Contains(payNo),
                IsCash = payNo == CashPayNo,
            });
        }

        // Cash line: counted + withdrawals − opening float, so the cashier's arithmetic is explicit.
        foreach (var c in Cashiers)
        {
            var cash = c.CashLine;
            if (cash is null && (c.Withdrawals != 0 || c.OpeningFloat != 0))
            {
                cash = new PaymentLine { PayNo = CashPayNo, Name = "Efectivo", Counted = true, IsCash = true };
                c.Lines.Add(cash);
            }
            if (cash is not null) cash.Declared = c.CashCounted + c.Withdrawals - c.OpeningFloat;
            c.Lines.Sort((a, b) => a.IsCash != b.IsCash ? (a.IsCash ? -1 : 1) : a.PayNo.CompareTo(b.PayNo));
        }
    }

    // "Venta 08:06 a 20:26" -> ("Venta", "08:06 a 20:26")
    private static readonly System.Text.RegularExpressions.Regex ScheduleRx =
        new(@"^(.*?)\s*(\d{1,2}:\d{2}\s+a\s+\d{1,2}:\d{2})\s*$");

    private void AttachActivity(List<dynamic> rows)
    {
        foreach (IDictionary<string, object> r in rows)
        {
            var cashier = Cashiers.FirstOrDefault(c => c.Code == Fmt.S(r["OperNo"]));
            if (cashier is null) continue;
            var type = Fmt.S(r["TipoTicket"]).Trim();
            var schedule = "";
            var m = ScheduleRx.Match(type);
            if (m.Success) { type = m.Groups[1].Value.Trim(); schedule = m.Groups[2].Value; }
            cashier.Activity.Add(new Activity
            {
                Terminal = Fmt.S(r["TermNo"]),
                TicketType = type,
                Schedule = schedule,
                Tickets = (int)Fmt.D(r["Tickets"]),
                Amount = Fmt.D(r["Importe"]),
                Alert = (int)Fmt.D(r["Alert"]),
            });
        }
    }

    /// <summary>Returns without original ticket in the whole store (Alert = 2).</summary>
    public IEnumerable<(Cashier Cashier, Activity Activity)> ReturnsWithoutOriginal =>
        Cashiers.SelectMany(c => c.ReturnsWithoutOriginal.Select(a => (c, a)));

    /// <summary>Card terminals (non-integrated POS) whose declared amount differs.</summary>
    public IEnumerable<(Cashier Cashier, PaymentLine Line)> CardIssues =>
        Cashiers.SelectMany(c => c.NonCashIssues.Select(l => (c, l)));

    // ---------- formatting helpers for the view ----------

    /// <summary>1.234,56 € — or an em dash for zero, so empty cells do not add noise.</summary>
    public static string Money(decimal v, bool dashForZero = false) =>
        dashForZero && v == 0 ? "—" : v.ToString("N2", Es).Replace("-", "−") + " €";

    /// <summary>Signed difference with the word that explains it.</summary>
    public static string DifferenceText(decimal v) => v switch
    {
        0 => "0,00 €",
        > 0 => "+" + v.ToString("N2", Es) + " € sobra",
        _ => "−" + Math.Abs(v).ToString("N2", Es) + " € falta",
    };

    /// <summary>Signed amount without the explaining word ("+135,86 €"), for large KPI figures.</summary>
    public static string SignedMoney(decimal v) => v switch
    {
        0 => "0,00 €",
        > 0 => "+" + v.ToString("N2", Es) + " €",
        _ => "−" + Math.Abs(v).ToString("N2", Es) + " €",
    };

    public static string DifferenceWord(decimal v) => v > 0 ? "sobra" : v < 0 ? "falta" : "";

    public static (string Css, string Icon, string Label) StatusInfo(CashStatus s) => s switch
    {
        CashStatus.Ok => ("ok", "✓", "Cuadra"),
        CashStatus.Tolerance => ("tol", "≈", "Tolerancia"),
        CashStatus.Review => ("rev", "!", "Revisar"),
        _ => ("ko", "✕", "Descuadre"),
    };
}
