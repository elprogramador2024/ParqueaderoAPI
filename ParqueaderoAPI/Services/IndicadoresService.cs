using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public class IndicadoresService : IIndicadoresService
    {
        private readonly ApplicationDbContext _db;
        public IndicadoresService(ApplicationDbContext db)
        {
            _db = db;
        }

        

        public List<VehiculoCant> GetTopVehiculos(int parqueaderoId, int cantVehiculos)
        {
            List<VehiculoCant> vehiculos = (from registroVehiculo in _db.RegistroVehiculo.Where(x => x.ParqueaderoId == parqueaderoId)
                             join vehiculo in _db.Vehiculo on registroVehiculo.VehiculoId equals vehiculo.Id
                             group vehiculo by vehiculo.Id into groupedVehiculos
                             select new VehiculoCant
                             {
                                 Id = groupedVehiculos.First().Id,
                                 Placa = groupedVehiculos.First().Placa,
                                 CantRegistrado = groupedVehiculos.Count()
                             })
                            .OrderByDescending(v => v.CantRegistrado)
                            .Take(cantVehiculos)
                            .ToList();

            return vehiculos;
        }

        public List<Vehiculo> GetVehPrimeraVez(int parqueaderoId)
        {
            List<Vehiculo> vehiculos = (from vehiculo in _db.Vehiculo.Where((v) => v.ParqueaderoId == parqueaderoId)
                             join registroVehiculo in _db.RegistroVehiculo on vehiculo.Id equals registroVehiculo.VehiculoId into registros
                             from registro in registros.DefaultIfEmpty()
                             where registro == null
                             select new Vehiculo
                             {
                                 Id = vehiculo.Id,
                                 Placa = vehiculo.Placa,
                                 FechaIngreso = vehiculo.FechaIngreso,
                                 ParqueaderoId = vehiculo.ParqueaderoId
                             }).ToList();

            return vehiculos;
        }

        public IngresosParqueadero GetGananciasByParqueadero(Parqueadero parqueadero, DateTime fechaIni, DateTime fechaFin)
        {
            List<RegistroVehiculo> registros = _db.RegistroVehiculo.Where(x => x.ParqueaderoId == parqueadero.Id && x.FechaIngreso >= fechaIni && x.FechaSalida <= fechaFin).ToList();
            double ganancias = registros.Sum((r) => (r.FechaSalida - r.FechaIngreso).TotalHours * parqueadero.CostoHora);

            return new IngresosParqueadero() { ParqueaderoId = parqueadero.Id, TotalIngresos = Math.Round(ganancias, 2) };
        }

        public List<IngresosUser> GetTopGanaciasBySocio(DateTime fechaIni, DateTime fechaFin, int cantRegistros)
        {
            var registros = (from registroVehiculo in _db.RegistroVehiculo.Where(x => x.FechaIngreso >= fechaIni && x.FechaSalida <= fechaFin)
                             join parqueadero in _db.Parqueadero on registroVehiculo.ParqueaderoId equals parqueadero.Id
                             select new
                             {
                                 registro = registroVehiculo,
                                 socioId = parqueadero.SocioId,
                                 costoHora = parqueadero.CostoHora
                             }
                            ).ToList();

            List<IngresosUser> topIngresosUser = registros.GroupBy(r => r.socioId).Select(g =>
                            new IngresosUser()
                            {
                                SocioId = g.First().socioId,
                                TotalIngresos = Math.Round(g.Sum((r) => (r.registro.FechaSalida - r.registro.FechaIngreso).TotalHours * r.costoHora), 2)
                            }).OrderByDescending(g => g.TotalIngresos)
                            .Take(cantRegistros)
                            .ToList();

            return topIngresosUser;
        }

        public List<IngresosParqueadero> GetTopGanaciasByParq(DateTime fechaIni, DateTime fechaFin, int cantRegistros)
        {
            var registros = (from registroVehiculo in _db.RegistroVehiculo.Where(x => x.FechaIngreso >= fechaIni && x.FechaSalida <= fechaFin)
                             join parqueadero in _db.Parqueadero on registroVehiculo.ParqueaderoId equals parqueadero.Id
                             select new
                             {
                                 registro = registroVehiculo,
                                 parqueaderoId = parqueadero.Id,
                                 costoHora = parqueadero.CostoHora
                             }
                            ).ToList();

            List<IngresosParqueadero> topIngresosParq = registros.GroupBy(r => r.parqueaderoId).Select(g =>
                            new IngresosParqueadero()
                            {
                                ParqueaderoId = g.First().parqueaderoId,
                                TotalIngresos = Math.Round(g.Sum((r) => (r.registro.FechaSalida - r.registro.FechaIngreso).TotalHours * r.costoHora), 2)
                            }).OrderByDescending(g => g.TotalIngresos)
                            .Take(cantRegistros)
                            .ToList();

            return topIngresosParq;
        }
    }
}
