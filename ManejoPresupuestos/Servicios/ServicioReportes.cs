using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{ 
    public interface IServicioReportes
    {
        Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int anio, dynamic ViewBag);
    }

    public class ServicioReportes: IServicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;

        public ServicioReportes(IRepositorioTransacciones repositorioTransacciones)
        {
            this.repositorioTransacciones = repositorioTransacciones;
        }

        public async Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int anio, dynamic ViewBag)
        {
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

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;

            var modelo = await repositorioTransacciones.ObtenerTransaccionesPorSemanas(parametro);
            return modelo;
        }
    }
}
