using Microsoft.AspNetCore.Identity;

namespace ParqueaderoAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
    }

    public class IngresosUser
    {
        public string SocioId { get; set; }
        public double TotalIngresos { get; set; }
    }
}
