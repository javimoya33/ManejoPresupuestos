using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Categoria>> ObtenerTipoOperacion(int usuarioId, TipoOperacion tipoOperacionId);
    }

    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string connectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                VALUES (@Nombre, @TipoOperacion, @UsuarioId);
                
                SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Categoria>(
                @$"SELECT * FROM Categorias
                WHERE UsuarioId = @usuarioId
                ORDER BY Nombre
                OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY", new { usuarioId });
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> ObtenerTipoOperacion(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Categoria>(
                "SELECT * FROM Categorias " +
                "WHERE UsuarioId = @usuarioId " +
                "AND TipoOperacionId = @tipoOperacionId", new {usuarioId, tipoOperacionId});
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                @"SELECT * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId", 
                new {id, usuarioId});
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"UPDATE Categorias
                                            SET Nombre = @Nombre, TipoOperacion = @TipoOperacion
                                            WHERE Id = @Id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id", new {id});
        }
    }
}
