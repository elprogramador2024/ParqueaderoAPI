using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public interface IIndicadoresService
    {
        public List<VehiculoCant> GetTopVehiculos(int parqueaderoId, int cantVehiculos);
        public List<Vehiculo> GetVehPrimeraVez(int parqueaderoId);
        public IngresosParqueadero GetGananciasByParqueadero(Parqueadero parqueadero, DateTime fechaIni, DateTime fechaFin);
        public List<IngresosUser> GetTopGanaciasBySocio(DateTime fechaIni, DateTime fechaFin, int cantRegistros);
        public List<IngresosParqueadero> GetTopGanaciasByParq(DateTime fechaIni, DateTime fechaFin, int cantRegistros);
    }
}
