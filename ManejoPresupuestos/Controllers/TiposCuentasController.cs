using Dapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace ManejoPresupuestos.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas1;
        private readonly IServicioUsuarios servicioUsuarios1;


        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas1 = repositorioTiposCuentas;
            this.servicioUsuarios1 = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas1.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuarios1.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas1.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");

                return View(tipoCuenta);
            }

            await repositorioTiposCuentas1.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas1.ObtTipoCuentaPorId(id, usuarioId);


            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas1.ObtTipoCuentaPorId(tipoCuenta.Id, usuarioId);

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas1.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas1.ObtTipoCuentaPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas1.ObtTipoCuentaPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas1.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas1.Existe(nombre, usuarioId);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios1.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas1.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id).ToList();

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
                new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await repositorioTiposCuentas1.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
