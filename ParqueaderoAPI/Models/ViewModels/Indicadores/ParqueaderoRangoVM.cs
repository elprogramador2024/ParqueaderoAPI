using ParqueaderoAPI.Utility;
using System.ComponentModel.DataAnnotations;

namespace ParqueaderoAPI.Models.ViewModels.Indicadores
{
    public class ParqueaderoRangoVM
    {
        [Required]
        public int ParqueaderoId { get; set; }
        [Required]
        [ValidEnum<RangoDias>]
        public RangoDias RangoDias { get; set; }
    }

    public enum RangoDias
    {
        DIARIO = 1,
        SEMANAL = 7,
        MENSUAL = 30,
        ANUAL = 365
    }
}
