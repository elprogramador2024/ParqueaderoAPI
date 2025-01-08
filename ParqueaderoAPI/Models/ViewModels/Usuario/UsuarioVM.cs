using ParqueaderoAPI.Utility;
using System.ComponentModel.DataAnnotations;

namespace ParqueaderoAPI.Models.ViewModels.Usuario
{
    public class UsuarioVM
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Rol Usuario")]
        [ValidRol]
        public string Rol { get; set; }
    }

    public class RegistroUsuarioVM : UsuarioVM
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "El {0} debe tener una longitud mínima de {2} carateres", MinimumLength = 5)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Password")]
        [Compare("Password", ErrorMessage = "Los passwords no son iguales")]
        public string ConfirmPassword { get; set; }
    }
}
