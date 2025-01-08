using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.Comunes;
using ParqueaderoAPI.Services;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParqueaderoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehiculoController : ControllerBase
    {
        #region Propiedades
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Constructores
        public VehiculoController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        #endregion

        #region Metodos
        /// <summary>
        /// Obtiene una lista de vehículos asociados a un parqueadero específico.
        /// </summary>
        /// <param name="parqueaderoId">ID del parqueadero del que se desea obtener los vehículos.</param>
        /// <returns>Lista de vehículos asociados al parqueadero.</returns>
        /// <response code="200">Lista de vehículos (List Vehiculo)</response>
        /// <response code="400">Parqueadero no existe o parqueadero no asociado a su cuenta con rol SOCIO (mensaje)</response>
        /// <remarks>
        /// - Los usuarios con rol **ADMIN** pueden consultar cualquier parqueadero.  
        /// - Los usuarios con rol **SOCIO** solo pueden consultar parqueaderos asociados a su cuenta.  
        /// </remarks>
        [HttpGet("VehiculosByParqueadero")]
        [Authorize(Roles = "ADMIN,SOCIO")]
        public async Task<IActionResult> GetVehiculosByParqueadero(int parqueaderoId)
        {
            var user_role = User.FindFirst(ClaimTypes.Role)?.Value;
            var parqueadero = await ParqueaderoExists(parqueaderoId);

            if (user_role == "SOCIO")
            {
                string socio_id = User.FindFirst(ClaimTypes.Actor)?.Value;

                if (!parqueadero.SocioId.Equals(socio_id))
                    return BadRequest(new { mensaje = "No puede ver detalles, parqueadero no asociado a su cuenta" });
            }

            if (parqueadero == null)
                return BadRequest(new { mensaje = "Parqueadero no existe" });

            List<Vehiculo> vehiculos = new VehiculoService(_db).GetVehiculosByParqueadero(parqueaderoId);
                     
            return Ok(vehiculos);
        }
        #endregion

        #region Metodos Auxiliares
        private async Task<Parqueadero> ParqueaderoExists(int parqueaderoId)
        {
            return await _db.Parqueadero.FindAsync(parqueaderoId);
        }
        #endregion
    }
}
