using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public interface IVehiculoService
    {
        public List<Vehiculo> GetVehiculosByParqueadero(int parqueaderoId);
        public int Upsert(Vehiculo vehiculo);
    }
}
