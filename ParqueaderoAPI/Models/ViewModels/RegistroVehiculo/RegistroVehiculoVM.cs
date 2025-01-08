using System.ComponentModel.DataAnnotations;
using ParqueaderoAPI.Utility;

namespace ParqueaderoAPI.Models.ViewModels.RegistroVehiculo
{
    public class RegistroVehiculoVM
    {
        [Required]
        public int ParqueaderoId { get; set; }
        [Required]
        [StringLength(6, ErrorMessage = "La placa debe tener de 6 caracteres de longitud")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "La placa debe ser alfanúmerica, no se permite carateres especiales, ni la letra ñ")]
        public string Placa { get; set; }
        [Required]
        [ValidEnum<TipoRegistro>]
        public TipoRegistro TipoRegistro { get; set; }
    }

    public enum TipoRegistro
    {
        INGRESO,
        SALIDA
    }
}