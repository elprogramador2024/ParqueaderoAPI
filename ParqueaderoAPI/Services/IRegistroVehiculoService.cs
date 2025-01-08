using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public interface IRegistroVehiculoService
    {
        public int Insert(RegistroVehiculo registroVehiculo);
    }
}