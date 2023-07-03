using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<IEnumerable<ObtenerTransaccionesPorMes>> ObtenerTransaccionesPorMes(int usuarioId, int anio);
        Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerTransaccionesPorSemanas(ParametrosObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuarioId(ParametrosObtenerTransaccionesPorUsuario parametro);
    }

    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                "Transacciones_Insertar", new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                }, 
                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Transaccion>(
                @"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.Id = t.CuentaId
                WHERE t.CuentaId = @CuentaId 
                AND t.UsuarioId = @UsuarioId
                AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuarioId(ParametrosObtenerTransaccionesPorUsuario parametro)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Transaccion>(
                @"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId, Nota
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.Id = t.CuentaId
                WHERE t.UsuarioId = @UsuarioId
                AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                ORDER BY t.FechaTransaccion DESC", parametro);
        }

        public async Task<IEnumerable<ObtenerTransaccionesPorSemana>> ObtenerTransaccionesPorSemanas(
            ParametrosObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ObtenerTransaccionesPorSemana>(
                @"SELECT datediff(d, @FechaInicio, FechaTransaccion) / 7 + 1 as Semana,
                SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId
                AND FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                GROUP BY datediff(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ObtenerTransaccionesPorMes>> ObtenerTransaccionesPorMes(int usuarioId, int anio)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ObtenerTransaccionesPorMes>(
                @"SELECT MONTH(FechaTransaccion) as Mes, SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId 
                AND YEAR(FechaTransaccion) = @Anio
                GROUP BY Month(FechaTransaccion), cat.TipoOperacionId", new {usuarioId, anio});
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CuentaId,
                    transaccion.CategoriaId, 
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT Transacciones.*, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.Id = @Id
                AND Transacciones.UsuarioId = @UsuarioId",
                new {id, usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
