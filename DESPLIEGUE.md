# Intranet Hiper Centro — ASP.NET Core (.NET 10 LTS)

Migración del ASP clásico (`F:\Webs_IIS\Dominio`) a Razor Pages sobre IIS,
manteniendo autenticación Windows (usuario de dominio) y autorización por grupos AD.

## Estructura

| Original (ASP clásico)                              | Nuevo (Razor Pages)                      |
|-----------------------------------------------------|------------------------------------------|
| `default.asp`                                       | `/` (Pages/Index)                        |
| `MadisaNetR2/Contabilidad/CuadreCajasResum.asp` (*) | `/Contabilidad` (selector fecha/tienda)  |
| `MadisaNetR2/Contabilidad/CuadreCajasList.asp`      | `/Contabilidad/CuadreCajas` (port fiel)  |
| — (rediseño del cuadre, en pruebas)                 | `/Contabilidad/Cuadre`                   |
| `MadisaNetR2/Contabilidad/Informe Balanzas.asp`     | `/Contabilidad/InformeBalanzas`          |
| `MadisaNetR2/Contabilidad/Informe Cierre.asp`       | `/Contabilidad/InformeCierre`            |
| `MadisaNetR2/Restringida/ResumenVentas.asp`         | `/Restringida/ResumenVentas`             |
| `MadisaNetR2/Restringida/TarjetasCredito.asp`       | `/Restringida/TarjetasCredito`           |
| `MadisaNetR2/Restringida/Tickets.asp`               | `/Restringida/Tickets`                   |
| `MadisaNetR2/Restringida/Tickets_Caja.asp`          | `/Restringida/TicketsCaja`               |
| `Informatica/inicio.asp`                            | `/Informatica`                           |
| `Informatica/MadisaControl.asp` (frameset)          | `/Informatica/MadisaControl`             |
| `Informatica/ControlGeneral.asp` (frameset)         | `/Informatica/ControlGeneral`            |
| `Informatica/MadisaSinc.asp`                        | `/Informatica/Sinc`                      |
| `Informatica/MadisaTransaccion.asp`                 | `/Informatica/Alertas`                   |
| `Informatica/MadisaCargaVentas.asp`                 | `/Informatica/CargaVentas`               |
| `Informatica/MadisaCajasFueraLinea.asp`             | `/Informatica/CajasFueraLinea`           |
| `Informatica/HorarioSinc.asp`                       | `/Informatica/HorarioSinc`               |
| `Informatica/Etiquetas.asp`                         | `/Informatica/Etiquetas`                 |
| `Informatica/BloqueoSql.asp`                        | `/Informatica/BloqueoSql`                |
| `Informatica/LogErrorPda.asp`                       | `/Informatica/LogErrorPda`               |
| `Informatica/Ping.asp` (frameset de .txt)           | `/Informatica/Ping`                      |

Los parámetros de QueryString se mantienen (`?v1=fecha&v2=alm&v3=...&v4=...&support=...`),
por lo que los enlaces existentes solo cambian la ruta base.

## Bases de datos

| Nombre en la app | DSN ODBC antiguo | Base de datos real | Servidor     |
|------------------|------------------|--------------------|--------------|
| `NavR2`          | NavR2            | **NAVHM**          | 192.168.1.16 |
| `MadisaNet`      | Madisa_Net       | **MadisaNet**      | 192.168.1.16 |

## Seguridad

- **Autenticación**: Windows (Negotiate/Kerberos). Sin pantalla de login, igual que ahora.
- **Autorización**: las ACL NTFS de las carpetas se sustituyen por grupos AD definidos en
  `appsettings.json → Autorizacion:Grupos`:
  - `HIPERCENTRO\Informatica`  → carpeta `/Informatica`
  - `HIPERCENTRO\Contabilidad` → carpeta `/Contabilidad`
  - `HIPERCENTRO\Restringida`  → carpeta `/Restringida`

  Son **provisionales**: al decidir los nombres definitivos basta cambiar el JSON
  (sin recompilar, reciclar el app pool).
- **SQL**: todas las llamadas son stored procedures con parámetros tipados (Dapper).
  Desaparece la inyección SQL por concatenación y las credenciales en claro de
  `Connections/Conexiones_Odbc.asp`. En producción la conexión usa `Integrated Security`
  con la identidad del Application Pool.

## Desarrollo (fuera del dominio)

`appsettings.Development.json` — **solo se carga en entorno Development**, nunca en producción:

- `Autorizacion:OmitirGruposAD: true` → las policies solo exigen usuario Windows autenticado
  (permite probar sin pertenecer a los grupos AD).
- `BaseDeDatos:Demo` → `true` usa datos de ejemplo sin conectar a SQL Server; `false` conecta
  al servidor real con el usuario SQL del ASP clásico.

```powershell
dotnet run          # http://localhost:5080
```

## Despliegue en IIS (paso a paso)

1. **Servidor** (una sola vez):
   - Instalar el **.NET 10 Hosting Bundle** (https://dotnet.microsoft.com/download/dotnet/10.0 → "Hosting Bundle").
   - `iisreset`.

2. **Cuenta de servicio**: crear gMSA `HIPERCENTRO\svc-intranet$` (o cuenta de dominio normal)
   y darle permisos en SQL Server sobre las BD `NAVHM` y `MadisaNet`
   (EXECUTE sobre los esquemas `rep` y `asp`):
   ```sql
   CREATE LOGIN [HIPERCENTRO\svc-intranet$] FROM WINDOWS;
   USE MadisaNet; CREATE USER [HIPERCENTRO\svc-intranet$]; GRANT EXECUTE ON SCHEMA::rep TO [HIPERCENTRO\svc-intranet$];
   USE NAVHM;     CREATE USER [HIPERCENTRO\svc-intranet$]; GRANT EXECUTE ON SCHEMA::asp TO [HIPERCENTRO\svc-intranet$]; GRANT EXECUTE ON SCHEMA::rep TO [HIPERCENTRO\svc-intranet$];
   ```

3. **Application Pool** `IntranetCore`:
   - .NET CLR Version: **No Managed Code**
   - Identity: la gMSA del paso 2.

4. **Publicar**:
   ```powershell
   dotnet publish -c Release -o F:\Webs_IIS\IntranetCore
   ```

5. **Subaplicación IIS**: en el sitio `Dominio` actual, añadir aplicación
   `Alias: Intranet` → ruta física `F:\Webs_IIS\IntranetCore`, app pool `IntranetCore`.

6. **Autenticación** de la subaplicación `Intranet`:
   - Windows Authentication: **Habilitada** (Negotiate, NTLM)
   - Anonymous Authentication: **Deshabilitada**

7. **Configurar** `appsettings.json` en el servidor:
   - `Autorizacion:Grupos`: nombres definitivos de los grupos AD.
   - `Ping:Carpeta`: carpeta donde el job deja los `PingCriticos.txt`, etc.

8. **Crear los grupos AD** y meter a los usuarios que hoy tienen ACL NTFS sobre cada carpeta.
   Para ver los permisos actuales:
   ```powershell
   icacls "F:\Webs_IIS\Dominio\MadisaNetR2\Restringida"
   icacls "F:\Webs_IIS\Dominio\MadisaNetR2\Contabilidad"
   icacls "F:\Webs_IIS\Dominio\Informatica"
   ```

9. **Transición**: redirigir cada `.asp` viejo a su página nueva (o sustituir los enlaces).
   Cuando no quede ningún `.asp` en uso: retirar WebKnight, los DSN ODBC y el sitio clásico.

## Nuevo cuadre de cajas (`/Contabilidad/Cuadre`)

Rediseño basado en una semana de datos reales (15–21/09/26, 234 tienda-días) y en una
auditoría de usabilidad. Usa los mismos stored procedures y da los mismos totales que
`/Contabilidad/CuadreCajas` (verificado al céntimo en 10 tienda-días), que se mantiene sin cambios.

- Cabecera con día anterior/siguiente y tienda anterior/siguiente (solo tiendas con cierres ese día,
  vía `rep.CuadreCajas_Resum`). Por defecto muestra el día de ayer.
- KPIs: teórico, declarado, diferencia de la tienda y cajeros a revisar.
- Avisos solo cuando hay algo: descuadres compensados entre cajeros, datáfonos que no cuadran,
  devoluciones sin ticket original (`Alert = 2` de `rep.CuadreCajas_ResumCajero`; `Alert = 1`
  solo indica que el cajero es responsable y se muestra como etiqueta neutra).
- Tabla de 7 columnas ordenada por |diferencia|; detalle expandible por cajero (desglose por forma
  de pago sin ceros, cálculo del efectivo, actividad por caja y accesos a tickets) en lugar de tooltips.
- Umbrales configurables en `appsettings.json` → `Cuadre:Tolerancia` (1 €) y `Cuadre:Descuadre` (20 €).
  Valores propuestos a partir de los datos: el 73 % de los cajeros descuadra < 1 € y el 97 % < 20 €.

## Notas

- (*) `CuadreCajasResum.asp` no estaba en la copia del sitio; `/Contabilidad` lo
  sustituye como punto de entrada: selector de fecha + tabla de tiendas con acceso
  directo a Cuadre, Ventas, Tarjetas, Cierre y Balanzas. Si el original hacía algo
  más (totales por tienda), pásame el .asp y lo replico.
- Pendiente de migrar (no estaba en esta carpeta): `Comunicados.asp`,
  `MADISA NET/Gestion/Cambios de pvp.asp` y todo `IntranetTiendas`.
- Las fechas se siguen pasando a los SP como texto `dd/MM/yy`, igual que hacía el ASP
  original, para no alterar el comportamiento de los procedimientos.
- La exportación Excel/Word (`?formato=Excel|Word`) se conserva como descarga del HTML
  (mismo mecanismo que usaba el original).
