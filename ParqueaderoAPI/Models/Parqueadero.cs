using ParqueaderoAPI.Models.ViewModels.Parqueadero;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParqueaderoAPI.Models
{

    public class ParqueaderoLst
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }

    }

    public class Parqueadero : ParqueaderoLst
    {
        [Required]
        public string Direccion { get; set; }
        [Required]
        public int Capacidad { get; set; }
        [Required]
        public double CostoHora { get; set; }
        [Required]
        public string SocioId { get; set; }

        [ForeignKey("SocioId")]
        public ApplicationUser User { get; set; }
    }

    public class IngresosParqueadero
    {
        public int ParqueaderoId { get; set; }
        public double TotalIngresos { get; set; }
    }

    public class ParqueaderoCant : ParqueaderoLst
    {
        public List<VehiculoCant> Vehiculos { get; set; } 
    }

    
}
