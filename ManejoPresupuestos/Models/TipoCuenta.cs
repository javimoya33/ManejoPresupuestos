using ManejoPresupuestos.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        /* Pruebas de otras validaciones por defecto */
        /*[Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Range(minimum:18, maximum:99, ErrorMessage = "El valor debe estar entre {1} y {2}")]
        public int Edad { get; set; }
        [Url(ErrorMessage = "El campo debe ser una URL válida")]
        public string URL { get; set; }
        [CreditCard(ErrorMessage = "La tarjeta de crédito no es válido")]
        [Display(Name = "Tarjeta de crédito")]
        public string TarjetaDeCredito { get; set; }*/
    }
}
