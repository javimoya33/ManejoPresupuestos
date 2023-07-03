using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using DataTable = System.Data.DataTable;

namespace ManejoPresupuestos.Controllers
{
    public class TransaccionesController: Controller
    {
        private readonly IServicioUsuarios servicioUsuarios1;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, 
            IRepositorioCategorias repositorioCategorias, IRepositorioTransacciones repositorioTransacciones, IMapper mapper,
            IServicioReportes servicioReportes)
        {
            this.servicioUsuarios1 = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.urlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (!string.IsNullOrEmpty(modelo.urlRetorno))
            {
                return LocalRedirect(modelo.urlRetorno);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("Index");
            }

            await repositorioTransacciones.Borrar(id);

            if (!string.IsNullOrEmpty(urlRetorno))
            {
                return LocalRedirect(urlRetorno);
            }

            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.ObtenerTipoOperacion(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);

            return Ok(categorias);
        }

        public async Task<IActionResult> Index(int mes, int anio)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || anio <= 1990)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var parametro = new ParametrosObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(parametro);

            var modelo = new ReportesTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReportesTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = HttpContext.Request.Path + HttpContext.Request.QueryString;

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int anio)
        {
            Console.WriteLine("Hola0 ");

            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            IEnumerable<ObtenerTransaccionesPorSemana> transaccionesPorSemana = 
                await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, anio, ViewBag);

            Console.WriteLine("Hola1 ");

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => 
                new ObtenerTransaccionesPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                        .Select(x => x.Monto).FirstOrDefault()
                }).ToList();

            Console.WriteLine("Hola2 ");

            if (anio == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);
            Console.WriteLine("Hola3 " + fechaReferencia);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anio, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(anio, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ObtenerTransaccionesPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanaViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            Console.WriteLine("Hola " + modelo.FechaReferencia);

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int anio)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            if (anio == 0)
            {
                anio = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerTransaccionesPorMes(usuarioId, anio);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ObtenerTransaccionesPorMes()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.tipoOperacionId == TipoOperacion.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Gasto = x.Where(x => x.tipoOperacionId == TipoOperacion.Gasto)
                        .Select(x => x.Monto).FirstOrDefault()
                }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(anio, mes, 1);

                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ObtenerTransaccionesPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Anio = anio;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ReporteExcel()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GeneralExcel(nombreArchivo, transacciones);
        }

        private FileResult GeneralExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto"),
            });

            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        nombreArchivo);
                }
            }
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAnio(int anio)
        {
            var fechaInicio = new DateTime(anio, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GeneralExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(100);
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GeneralExcel(nombreArchivo, transacciones);
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = start,
                    FechaFin = end
                });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto) ? "Red" : "Green"
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerTransaccionesPorUsuarioId(
                new ParametrosObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fecha,
                    FechaFin = fecha
                });

            return Json(transacciones);
        }
    }
}
