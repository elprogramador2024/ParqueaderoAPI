using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParqueaderoAPI.Models
{
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Placa { get; set; }

        public DateTime? FechaIngreso { get; set; }
        public int ParqueaderoId { get; set; }
    }
    
    public class RegistroVehiculo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime FechaIngreso { get; set; }
        public DateTime FechaSalida { get; set; }
        [Required]        
        public int ParqueaderoId { get; set; }
        [Required]
        public int VehiculoId { get; set; }

        [ForeignKey("ParqueaderoId")]
        public Parqueadero Parqueadero { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo Vehiculo { get; set; }
    }

    public class VehiculoCant
    {
        public int Id { get; set; }
        public string Placa { get; set; }
        public int CantRegistrado { get; set; }
    }

    
}
