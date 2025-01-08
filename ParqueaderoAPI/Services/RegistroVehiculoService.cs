using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public class RegistroVehiculoService : IRegistroVehiculoService
    {
        private readonly ApplicationDbContext _db;
        public RegistroVehiculoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public int Insert(RegistroVehiculo registroVehiculo)
        {
            _db.RegistroVehiculo.Add(registroVehiculo);
            return _db.SaveChanges();
        }
    }
}
