using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ParqueaderoAPI.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<ApplicationUser> Usuario { get; set; }
        public DbSet<Parqueadero> Parqueadero { get; set; }
        public DbSet<Vehiculo> Vehiculo { get; set; }
        public DbSet<RegistroVehiculo> RegistroVehiculo { get; set; }
    }     
}
