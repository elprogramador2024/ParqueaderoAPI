using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public class VehiculoService : IVehiculoService
    {
        private readonly ApplicationDbContext _db;
        public VehiculoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Vehiculo> GetVehiculosByParqueadero(int parqueaderoId)
        {
            return _db.Vehiculo.Where((v) => v.FechaIngreso != null && v.ParqueaderoId == parqueaderoId).ToList();
        }

        public int Upsert(Vehiculo vehiculo)
        {
            if (vehiculo.Id == 0)
                _db.Vehiculo.Add(vehiculo);
            else
                _db.Vehiculo.Update(vehiculo);

            return _db.SaveChanges();
        }
    }
}
