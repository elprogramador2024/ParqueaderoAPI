using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ParqueaderoAPI.Models.ViewModels.Parqueadero
{
    public class ParqueaderoVM
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Direccion { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad del parqueadero deber ser mínimo de {1}")]
        public int Capacidad { get; set; }
        [Required]
        public double CostoHora { get; set; }
        [Required]
        public string SocioId { get; set; }
    }
}
