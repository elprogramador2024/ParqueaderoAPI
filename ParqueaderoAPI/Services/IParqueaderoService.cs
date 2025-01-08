using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Services
{
    public interface IParqueaderoService
    {
        public (List<Parqueadero>, int) GetListPaginado(string user_role, string socio_id, int pgnum, int pgsize);
        public int Upsert(Parqueadero parqueadero);
        public int Delete(Parqueadero parqueadero);
    }
}
