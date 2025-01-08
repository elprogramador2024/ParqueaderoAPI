using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.Comunes;
using ParqueaderoAPI.Models.ViewModels.Parqueadero;
using ParqueaderoAPI.Models.ViewModels.Usuario;
using ParqueaderoAPI.Services;
using System.Net;
using System.Security.Claims;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParqueaderoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParqueaderoController : ControllerBase
    {
        #region Propiedades
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;        
        #endregion

        #region Constructores
        public ParqueaderoController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        #endregion

        #region Metodos
        /// <summary>
        /// Obtiene una lista paginada de parqueaderos.
        /// </summary>
        /// <param name="pgnum">Número de página (1 por defecto).</param>
        /// <param name="pgsize">Tamaño de página (5 por defecto).</param>
        /// <returns>Lista paginada de parqueaderos.</returns>
        /// <response code="200">Lista de parqueaderos (List Parqueadero)</response>
        /// <remarks>
        /// - Este endpoint permite a los usuarios con rol **ADMIN** o **SOCIO** obtener una lista paginada de parqueaderos.
        /// - Si el usuario tiene el rol **SOCIO**, solo verá parqueaderos asociados a su cuenta.
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = "ADMIN,SOCIO")]
        public async Task<IActionResult> GetAll(int pgnum, int pgsize)
        {
            var user_role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (pgnum <= 0) pgnum = 1;
            if (pgsize <= 0) pgsize = 5;

            string socio_id = User.FindFirst(ClaimTypes.Actor)?.Value;

            (List<Parqueadero> parqueaderos, int tot_items) = new ParqueaderoService(_db).GetListPaginado(user_role, socio_id, pgnum, pgsize);

            PgResponse<Parqueadero> response = new()
            {
                Total = tot_items,
                Pgnum = pgnum,
                Pgsize = pgsize,
                Totpages = (int)Math.Ceiling(tot_items / (double)pgsize),
                Lista = parqueaderos
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene una lista paginada de parqueaderos asociados a un socio específico.
        /// </summary>
        /// <param name="socioId">ID del socio.</param>
        /// <param name="pgnum">Número de página (1 por defecto).</param>
        /// <param name="pgsize">Tamaño de página (5 por defecto).</param>
        /// <returns>Lista paginada de parqueaderos asociados al socio.</returns>
        /// <response code="200">Lista de parqueaderos (List Parqueadero)</response>
        /// <remarks>
        /// - Este endpoint es accesible solo por usuarios con rol **ADMIN** y permite consultar los parqueaderos asociados a un socio específico.
        /// </remarks>
        [HttpGet("BySocio")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetBySocio(string socioId, int pgnum, int pgsize)
        {
            ApplicationUser user = await UserExists(socioId);

            if (user == null)
                return BadRequest("Usuario no existe");

            if (pgnum <= 0) pgnum = 1;
            if (pgsize <= 0) pgsize = 5;

            (List<Parqueadero> parqueaderos, int tot_items) = new ParqueaderoService(_db).GetListPaginado("SOCIO", user.Id, pgnum, pgsize);

            PgResponse<Parqueadero> response = new()
            {
                Total = tot_items,
                Pgnum = pgnum,
                Pgsize = pgsize,
                Totpages = (int)Math.Ceiling(tot_items / (double)pgsize),
                Lista = parqueaderos
            };

            return Ok(response);
        }

        /// <summary>
        /// Inserta un nuevo parqueadero.
        /// </summary>
        /// <param name="model">Información del parqueadero a insertar.</param>
        /// <returns>Resultado de la operación.</returns>
        /// <response code="201">"Parqueadero creado exitosamente! (mensaje)</response>
        /// <response code="400">Error en la validación o en el proceso de inserción (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a los usuarios con rol **ADMIN** crear un nuevo parqueadero.
        /// - Se valida que el socio asociado exista antes de crear el parqueadero.
        /// </remarks>
        [HttpPost("insert")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Insert([FromBody] ParqueaderoVM model)
        {
            string error = await ValidarSocio(model.SocioId);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            Parqueadero parqueadero = new Parqueadero()
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Direccion = model.Direccion,
                Capacidad = model.Capacidad,
                CostoHora = model.CostoHora,
                SocioId = model.SocioId
            };
            
            var result = new ParqueaderoService(_db).Upsert(parqueadero);

            if (result > 0)
            {
                model.Id = parqueadero.Id;
                return StatusCode((int)HttpStatusCode.Created, new { mensaje = "Parqueadero creado exitosamente!", parqueadero = model });
            }

            return BadRequest(new { mensaje = "No fue posible insertar parqueadero" });
        }

        /// <summary>
        /// Actualiza la información de un parqueadero existente.
        /// </summary>
        /// <param name="model">Información del parqueadero a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        /// <response code="200">Parqueadero actualizado exitosamente! (mensaje)</response>
        /// <response code="400">Error de validación o actualización (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a los usuarios con rol **ADMIN** actualizar la información de un parqueadero existente.
        /// - Se valida que el socio asociado exista.
        /// </remarks>
        [HttpPut("update")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update([FromBody] ParqueaderoVM model)
        {
            string error = await ValidarSocio(model.SocioId);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            var parqueadero = _db.Parqueadero.Find(model.Id);

            if (parqueadero == null)
                return BadRequest(new { mensaje = "Parqueadero no existe" });            

            parqueadero.Nombre = model.Nombre;
            parqueadero.Direccion = model.Direccion;
            parqueadero.Capacidad = model.Capacidad;
            parqueadero.CostoHora = model.CostoHora;
            parqueadero.SocioId = model.SocioId;

            var result = new ParqueaderoService(_db).Upsert(parqueadero);

            if (result > 0)
            {
                return Ok(new { mensaje = "Parqueadero actualizado exitosamente!", parqueadero = model });
            }

            return BadRequest(new { mensaje = "No fue posible actualizar parqueadero" });
        }

        /// <summary>
        /// Elimina un parqueadero existente.
        /// </summary>
        /// <param name="id">ID del parqueadero a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        /// <response code="200">Parqueadero eliminado exitosamente! (mensaje)</response>
        /// <response code="400">El parqueadero no existe o no se pudo eliminar (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a los usuarios con rol **ADMIN** eliminar un parqueadero existente.
        /// </remarks>
        [HttpDelete("delete")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var parqueadero = _db.Parqueadero.Find(id);

            if (parqueadero == null)
                return BadRequest(new { mensaje = "Parqueadero no existe" });

            var result = new ParqueaderoService(_db).Delete(parqueadero);

            if (result > 0)
            {
                return Ok(new { mensaje = "Parqueadero eliminado exitosamente!" });
            }

            return BadRequest(new { mensaje = "No fue posible eliminar parqueadero" });
        }
        #endregion

        #region Metodos Auxiliares
        private async Task<ApplicationUser> UserExists(string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId))
                return null;

            return await _userManager.FindByIdAsync(usuarioId);
        }

        private async Task<string> ValidarSocio(string socioId)
        {
            ApplicationUser user = await UserExists(socioId);

            if (user == null)
                return "Usuario (Socio) no existe";

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("SOCIO"))
                return "Id no corresponde a un socio";

            return string.Empty;
        }
        #endregion
    }
}
