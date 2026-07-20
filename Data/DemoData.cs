using System.Dynamic;

namespace Intranet.Data;

/// <summary>
/// Datos de ejemplo para probar la web sin conectividad con SQL Server
/// (modo "BaseDeDatos:Demo" en appsettings.Development.json).
/// Devuelve filas con las mismas columnas que los stored procedures reales.
/// </summary>
public static class DemoData
{
    private static dynamic Row(params (string K, object? V)[] campos)
    {
        IDictionary<string, object?> d = new ExpandoObject();
        foreach (var (k, v) in campos) d[k] = v;
        return d;
    }

    public static List<dynamic> Get(string procedimiento) => procedimiento switch
    {
        "rep.CuadreCajas" =>
        [
            Row(("PayNo", 1), ("OperNo", 12), ("StoreNo", 215), ("DescOper", "ANA GARCÍA"), ("Pago", 0m), ("Importe", 1543.20m), ("Declarado", 1540.00m), ("Entrada", 100.00m), ("Retirada", 800.00m), ("Retirada2", 0m)),
            Row(("PayNo", 2), ("OperNo", 12), ("StoreNo", 215), ("DescOper", "ANA GARCÍA"), ("Pago", 0m), ("Importe", 920.45m), ("Declarado", 920.45m), ("Entrada", 0m), ("Retirada", 0m), ("Retirada2", 0m)),
            Row(("PayNo", 10), ("OperNo", 12), ("StoreNo", 215), ("DescOper", "ANA GARCÍA"), ("Pago", 0m), ("Importe", 25.00m), ("Declarado", 25.00m), ("Entrada", 0m), ("Retirada", 0m), ("Retirada2", 0m)),
            Row(("PayNo", 1), ("OperNo", 17), ("StoreNo", 215), ("DescOper", "LUIS PÉREZ"), ("Pago", 12.50m), ("Importe", 2210.80m), ("Declarado", 2205.30m), ("Entrada", 100.00m), ("Retirada", 1500.00m), ("Retirada2", 50.00m)),
            Row(("PayNo", 2), ("OperNo", 17), ("StoreNo", 215), ("DescOper", "LUIS PÉREZ"), ("Pago", 0m), ("Importe", 1310.00m), ("Declarado", 1310.00m), ("Entrada", 0m), ("Retirada", 0m), ("Retirada2", 0m)),
            Row(("PayNo", 14), ("OperNo", 17), ("StoreNo", 215), ("DescOper", "LUIS PÉREZ"), ("Pago", 0m), ("Importe", 60.00m), ("Declarado", 55.00m), ("Entrada", 0m), ("Retirada", 0m), ("Retirada2", 0m)),
        ],
        "rep.CuadreCajas_Tarjetas" =>
        [
            Row(("Tipo", 2), ("DescPago", "VISA"), ("Teorico", 2230.45m), ("Declarado", 2230.45m)),
            Row(("Tipo", 6), ("DescPago", "MASTERCARD"), ("Teorico", 410.00m), ("Declarado", 395.00m)),
        ],
        "rep.CardPos_FechaAlm" =>
        [
            Row(("CentroAuto", "REDSYS"), ("Tipo", "VISA"), ("TipoOper", "Venta"), ("Amount", 2230.45m)),
            Row(("CentroAuto", "REDSYS"), ("Tipo", "MASTERCARD"), ("TipoOper", "Devolución"), ("Amount", -15.20m)),
        ],
        "rep.CuadreCajas_ResumCajero" =>
        [
            Row(("OperNo", 12), ("DescOperNo", "ANA GARCÍA"), ("TermNo", 3), ("TipoTicket", "Venta"), ("Tickets", 184), ("Prods", 1320), ("Importe", 2488.65m), ("Alert", 0)),
            Row(("OperNo", 17), ("DescOperNo", "LUIS PÉREZ"), ("TermNo", 5), ("TipoTicket", "Venta"), ("Tickets", 211), ("Prods", 1554), ("Importe", 3593.30m), ("Alert", 2)),
        ],
        "rep.Balanzas" =>
        [
            Row(("seccion", "FRUTERÍA"), ("Tecla", 12), ("Venta", 842.30m), ("Dto", 12.40m)),
            Row(("seccion", "CARNICERÍA"), ("Tecla", 14), ("Venta", 1290.75m), ("Dto", 31.10m)),
            Row(("seccion", "PESCADERÍA"), ("Tecla", 16), ("Venta", 655.00m), ("Dto", 8.00m)),
        ],
        "rep.Seccion" =>
        [
            Row(("OperatorNo", 12), ("Description", "ANA GARCÍA"), ("ItemNo", "100234"), ("descripcion", "ZAPATILLA DEPORTIVA T42"), ("cant", 2), ("importe", 39.90m)),
            Row(("OperatorNo", 17), ("Description", "LUIS PÉREZ"), ("ItemNo", "100871"), ("descripcion", "BOTA AGUA INFANTIL T30"), ("cant", 1), ("importe", 12.95m)),
        ],
        "rep.TeffFirma" =>
        [
            Row(("OperNo", 12), ("Operador", "ANA GARCÍA"), ("NumTarjetas", 4), ("NumTarjetasDevol", 0)),
            Row(("OperNo", 17), ("Operador", "LUIS PÉREZ"), ("NumTarjetas", 7), ("NumTarjetasDevol", 1)),
        ],
        "rep.CambiosPvp_Varios" =>
        [
            Row(("OperNo", 17), ("Operador", "LUIS PÉREZ"), ("Autorizado", "SUPERVISOR 3"), ("Alert", ""), ("Caja", 5), ("FechaHora", DateTime.Today.AddHours(11.5)), ("DescProd", "DETERGENTE 3L"), ("Cantidad", 1), ("Precio2", 6.95m), ("Precio", 8.50m), ("DescMotiv", "Etiqueta errónea"), ("Importe", 6.95m)),
            Row(("OperNo", 12), ("Operador", "ANA GARCÍA"), ("Autorizado", "12"), ("Alert", "Autoautorizado"), ("Caja", 3), ("FechaHora", DateTime.Today.AddHours(17.25)), ("DescProd", "ACEITE OLIVA 1L"), ("Cantidad", 2), ("Precio2", 7.20m), ("Precio", 7.99m), ("DescMotiv", "Promoción"), ("Importe", 14.40m)),
        ],
        "rep.VentasProg" =>
        [
            Row(("DescProg", "ALIMENTACIÓN"), ("Importe", 8240.55m), ("Ticks", 310), ("Prods", 2890)),
            Row(("DescProg", "DROGUERÍA"), ("Importe", 1875.20m), ("Ticks", 140), ("Prods", 410)),
            Row(("DescProg", "TEXTIL"), ("Importe", 960.00m), ("Ticks", 52), ("Prods", 87)),
        ],
        "rep.Ventas_NumTickets" =>
        [
            Row(("Ticks", 395)),
        ],
        "rep.CardPos_FechaAlm_List" =>
        [
            Row(("DateTime", DateTime.Today.AddHours(10.2)), ("TermNo", 3), ("OperNo", 12), ("Counter", 18432), ("OperationNumber", "001245"), ("AuthorizationNumber", "778812"), ("CardNumber", "**** **** **** 4412"), ("MetodoValidacion", "PIN"), ("AuthorizationCenter", "REDSYS"), ("CardType", "VISA"), ("Amount", 54.30m)),
            Row(("DateTime", DateTime.Today.AddHours(12.7)), ("TermNo", 5), ("OperNo", 17), ("Counter", 18501), ("OperationNumber", "001302"), ("AuthorizationNumber", "779034"), ("CardNumber", "**** **** **** 9087"), ("MetodoValidacion", "Contactless"), ("AuthorizationCenter", "REDSYS"), ("CardType", "MASTERCARD"), ("Amount", 21.15m)),
        ],
        "rep.CardPos_FechaAlm_Oper" =>
        [
            Row(("OperNo", 12), ("Tipo", "VISA"), ("TipoOper", "Venta"), ("Amount", 54.30m)),
            Row(("OperNo", 17), ("Tipo", "MASTERCARD"), ("TipoOper", "Venta"), ("Amount", 21.15m)),
        ],
        "rep.MovTicket" =>
        [
            Row(("Counter", 18432), ("TipoRegistro", "0CabeceraTicket"), ("TermNo", 3), ("EanTicket", "8400000184321"), ("FactSimplificada", "FS-18432"), ("TransType", "0"), ("Hora", DateTime.Today.AddHours(10.2)), ("Desc_trans_type", "Venta"), ("TicketDevolucion", ""), ("AutorizeNo", "0"), ("Pedido", ""), ("CustNo", 0), ("Cliente", ""), ("ImporteTicket", 23.85m), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 0), ("formapago", "")),
            Row(("Counter", 18432), ("TipoRegistro", "10producto"), ("TermNo", 3), ("ItemNo", "100234"), ("Quantity", 2), ("TruePrice", 7.95m), ("Amount", 15.90m), ("DescProd", "LECHE ENTERA 1L PACK6"), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 0), ("formapago", "")),
            Row(("Counter", 18432), ("TipoRegistro", "10producto"), ("TermNo", 3), ("ItemNo", "100871"), ("Quantity", 1), ("TruePrice", 7.95m), ("Amount", 7.95m), ("DescProd", "CAFÉ MOLIDO 250G"), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 0), ("formapago", "")),
            Row(("Counter", 18432), ("TipoRegistro", "30pago"), ("TermNo", 3), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 23.85m), ("formapago", "Efectivo")),
            Row(("Counter", 18501), ("TipoRegistro", "0CabeceraTicket"), ("TermNo", 3), ("EanTicket", "8400000185013"), ("FactSimplificada", "FS-18501"), ("TransType", "6"), ("Hora", DateTime.Today.AddHours(12.7)), ("Desc_trans_type", "Devolución"), ("TicketDevolucion", "0"), ("AutorizeNo", "0"), ("Pedido", ""), ("CustNo", 0), ("Cliente", ""), ("ImporteTicket", -7.95m), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 0), ("formapago", "")),
            Row(("Counter", 18501), ("TipoRegistro", "10producto"), ("TermNo", 3), ("ItemNo", "100871"), ("Quantity", -1), ("TruePrice", 7.95m), ("Amount", -7.95m), ("DescProd", "CAFÉ MOLIDO 250G"), ("TransTypeNo", "6"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", 0), ("formapago", "")),
            Row(("Counter", 18501), ("TipoRegistro", "30pago"), ("TermNo", 3), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("DescCambioPvp", ""), ("Cambio", "0"), ("Importe", -7.95m), ("formapago", "Efectivo")),
        ],
        "rep.MovTicket_Term" =>
        [
            Row(("Counter", 18460), ("TipoRegistro", "0CabeceraTicket"), ("TermNo", 5), ("FactSimplificada", "FS-18460"), ("DescOperator", "LUIS PÉREZ"), ("Hora", DateTime.Today.AddHours(11.1)), ("AutorizeNo", "0"), ("Pedido", ""), ("CustNo", 40012345), ("Cliente", "COMERCIAL EJEMPLO SL"), ("ImporteTicket", 36.25m), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("Orden", 0), ("Importe", 0), ("FormaPago", ""), ("Cambio", "0"), ("Desc_trans_type", "")),
            Row(("Counter", 18460), ("TipoRegistro", "10producto"), ("TermNo", 5), ("ItemNo", "200145"), ("Quantity", 5), ("TruePrice", 5.99m), ("Amount", 29.95m), ("DescProd", "PAPEL HIGIÉNICO 12R"), ("TransTypeNo", "0"), ("Orden", 0), ("Importe", 0), ("FormaPago", ""), ("Cambio", "0"), ("Desc_trans_type", "")),
            Row(("Counter", 18460), ("TipoRegistro", "20Iva"), ("TermNo", 5), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("Orden", 0), ("Importe", 29.96m), ("FormaPago", "IVA 21%"), ("Cambio", 6.29m), ("Desc_trans_type", "")),
            Row(("Counter", 18460), ("TipoRegistro", "31teff"), ("TermNo", 5), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("Orden", 0), ("Importe", 36.25m), ("FormaPago", "VISA"), ("Cambio", "0"), ("Desc_trans_type", "Aut. 778812")),
            Row(("Counter", 18460), ("TipoRegistro", "30pago"), ("TermNo", 5), ("ItemNo", ""), ("Quantity", 0), ("TruePrice", 0), ("Amount", 0), ("DescProd", ""), ("TransTypeNo", "0"), ("Orden", 0), ("Importe", 36.25m), ("FormaPago", "Tarjeta"), ("Cambio", "0"), ("Desc_trans_type", "")),
        ],
        "rep.TransRecibidas" =>
        [
            Row(("Alm", 1), ("DescAlm", "HIPER CENTRO 1"), ("HoraUltVenta", DateTime.Now.AddMinutes(-4).ToString("HH:mm:ss")), ("MinDif", 4)),
            Row(("Alm", 19), ("DescAlm", "HIPER CENTRO 19"), ("HoraUltVenta", DateTime.Now.AddMinutes(-2).ToString("HH:mm:ss")), ("MinDif", 2)),
            Row(("Alm", 215), ("DescAlm", "HIPER CENTRO 215"), ("HoraUltVenta", DateTime.Now.AddMinutes(-42).ToString("HH:mm:ss")), ("MinDif", 42)),
        ],
        "rep.Alertas" =>
        [
            Row(("DescAlerta", "Caja sin ventas > 30 min"), ("Alm", 215), ("Caja", 4), ("Texto", "Última transacción 11:02")),
            Row(("DescAlerta", "Diferencia cierre"), ("Alm", 19), ("Caja", 2), ("Texto", "Descuadre -5,50 €")),
        ],
        "rep.CargaVentas_Select" =>
        [
            Row(("FechaCarga", DateTime.Today.ToString("dd/MM/yyyy 06:15")), ("FechaNum", DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy")), ("Alm", 1), ("DescError", ""), ("Error", 0)),
            Row(("FechaCarga", DateTime.Today.ToString("dd/MM/yyyy 06:18")), ("FechaNum", DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy")), ("Alm", 19), ("DescError", "Fichero de ventas incompleto"), ("Error", 1)),
        ],
        "rep.CajasFueraLineaR2" =>
        [
            Row(("Error", 2)),
        ],
        "asp.Info_SincControlHorario" =>
        [
            Row(("Alm", 1), ("DescAlm", "HIPER CENTRO 1"), ("Equipo", "FICHAJE-01"), ("HoraSinc", DateTime.Now.AddMinutes(-12).ToString("dd/MM/yyyy HH:mm"))),
            Row(("Alm", 19), ("DescAlm", "HIPER CENTRO 19"), ("Equipo", "FICHAJE-19"), ("HoraSinc", DateTime.Now.AddHours(-3).ToString("dd/MM/yyyy HH:mm"))),
        ],
        "rep.InfoEtiquetas" =>
        [
            Row(("Fecha", DateTime.Today), ("Alm", 1), ("DescAlm", "HIPER CENTRO 1"), ("Normal", 240), ("OfertaFin", 35), ("OfertaIni", 58), ("Total", 333)),
            Row(("Fecha", DateTime.Today), ("Alm", 19), ("DescAlm", "HIPER CENTRO 19"), ("Normal", 198), ("OfertaFin", 22), ("OfertaIni", 41), ("Total", 261)),
        ],
        "asp.BloqueosSql" =>
        [
            Row(("PC", "PC-CONTA-02"), ("App", "Navision"), ("Usuario", "HIPERCENTRO\\mlopez"), ("Ip", "192.168.1.54"), ("PC_Bloqueado", "PC-ALMACEN-01"), ("App_Bloqueado", "Navision"), ("Usuario_Bloqueado", "HIPERCENTRO\\jruiz"), ("TiempoBloqSeg", 127), ("SentenciaSql", "UPDATE [Item Ledger Entry] SET ..."), ("SentenciaSql2", "SELECT * FROM [Item] WITH (UPDLOCK)")),
        ],
        "asp.ErrorLogPda" =>
        [
            Row(("Equipo", "PDA-07"), ("FechaHora", DateTime.Now.AddMinutes(-25).ToString("dd/MM/yyyy HH:mm")), ("Texto", "Timeout al confirmar traspaso 4471")),
            Row(("Equipo", "PDA-03"), ("FechaHora", DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm")), ("Texto", "Lectura EAN no encontrada: 8400000999999")),
        ],
        "asp.Almacen" =>
        [
            Row(("Alm", 1), ("DescAlm", "HIPER CENTRO 1")),
            Row(("Alm", 19), ("DescAlm", "HIPER CENTRO 19")),
            Row(("Alm", 215), ("DescAlm", "HIPER CENTRO 215")),
        ],
        "asp.PROC_Comunicados_AlmSelect_Pendientes" => [],
        _ => [],
    };
}
