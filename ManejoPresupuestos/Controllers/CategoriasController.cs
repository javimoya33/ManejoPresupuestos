using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestos.Controllers
{
    public class CategoriasController: Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuarios servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuarios = servicioUsuarios;
        }
        public async Task<IActionResult> Index(TipoOperacion tipoOperacion, PaginacionViewModel paginacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.Obtener(usuarioId, paginacion);
            var totalCategorias = await repositorioCategorias.Contar(usuarioId);

            var respuestaViewModel = new PaginacionRespuesta<Categoria>
            {
                Elementos = categorias,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalCategorias,
                BaseURL = Url.Action()
            };

            return View(respuestaViewModel);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Crear(categoria);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = repositorioCategorias.ObtenerPorId(categoriaEditar.Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCategorias.Actualizar(categoriaEditar);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            Console.WriteLine("Llego aquí");
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            Console.WriteLine("Llego aquí " + usuarioId);
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            Console.WriteLine("Llego aquí " + usuarioId + " - " + id);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = repositorioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategorias.Borrar(id);

            return RedirectToAction("Index");
        }
    }
}
