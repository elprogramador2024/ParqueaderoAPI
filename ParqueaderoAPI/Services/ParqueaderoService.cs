using ParqueaderoAPI.Models;
using System.Drawing;
using System.Security.Claims;
using System;

namespace ParqueaderoAPI.Services
{
    public class ParqueaderoService : IParqueaderoService
    {
        private readonly ApplicationDbContext _db;
        public ParqueaderoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public (List<Parqueadero>, int) GetListPaginado(string user_role, string socio_id, int pgnum, int pgsize)
        {
            List<Parqueadero> parqueaderos = new();
            int tot_items = 0;

            if (user_role == "ADMIN")
            {
                parqueaderos = _db.Parqueadero.Skip((pgnum - 1) * pgsize).Take(pgsize).ToList();
                tot_items = _db.Parqueadero.Count();
            }

            if (user_role == "SOCIO")
            {
                parqueaderos = _db.Parqueadero.Where((t) => t.SocioId == socio_id).Skip((pgnum - 1) * pgsize).Take(pgsize).OrderBy((t) => t.Id).ToList();
                tot_items = _db.Parqueadero.Count((t) => t.SocioId == socio_id);
            }

            return (parqueaderos, tot_items);
        }

        public int Upsert(Parqueadero parqueadero)
        {
            if (parqueadero.Id == 0)
                _db.Parqueadero.Add(parqueadero);
            else
                _db.Parqueadero.Update(parqueadero);

            return _db.SaveChanges();
        }

        public int Delete(Parqueadero parqueadero)
        {
            _db.Parqueadero.Remove(parqueadero);
            return _db.SaveChanges();
        }
    }
}
