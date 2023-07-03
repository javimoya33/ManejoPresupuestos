using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:50, ErrorMessage = "No puede ser mayor de {1} caracteres")]
        public string Nombre { get; set; }

        [Display(Name = "Tipo de Operación")]
        public TipoOperacion tipoOperacion { get; set; }
        public int UsuarioId { get; set; }

    }
}
