using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.ViewModels.Indicadores;
using ParqueaderoAPI.Services;
using ParqueaderoAPI.Utility;
using System.Security.Claims;

namespace ParqueaderoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndicadoresController : ControllerBase
    {
        #region Propiedades
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Constructores
        public IndicadoresController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        #endregion

        #region Metodos
        /// <summary>
        /// Obtiene los 10 vehículos más frecuentes en un parqueadero específico.
        /// </summary>
        /// <param name="parqueaderoId">ID del parqueadero.</param>
        /// <returns>Lista de los 10 vehículos más frecuentes.</returns>
        /// <response code="200">Lista de los 10 vehículos mas registrados en un parqueadero y su cantidad de registros (List VehiculoCant)</response>
        /// <response code="400">Error al validar el parqueadero (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a usuarios con rol **ADMIN** o **SOCIO** obtener los vehículos más frecuentes en un parqueadero.
        /// - El límite predeterminado es 10.
        /// - Se valida que el parqueadero exista antes de realizar la consulta.
        /// </remarks>
        [HttpGet("topvehiculos")]
        [Authorize(Roles = "ADMIN,SOCIO")]
        public async Task<IActionResult> GetTopVehiculos(int parqueaderoId)
        {
            string error = await ValidarParqueadero(parqueaderoId);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            List<VehiculoCant> vehiculos = new IndicadoresService(_db).GetTopVehiculos(parqueaderoId, 10);
            return Ok(vehiculos);

        }

        /// <summary>
        /// Obtiene una lista de vehículos ingresados por primera vez a un parqueadero.
        /// </summary>
        /// <param name="parqueaderoId">ID del parqueadero.</param>
        /// <returns>Lista de vehículos que ingresaron por primera vez.</returns>
        /// <response code="200">Lista de vehiculos parqueados por primera vez registrados (List Vehiculo)</response>
        /// <response code="400">Error al validar el parqueadero (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a usuarios con rol **ADMIN** o **SOCIO** consultar vehículos que ingresaron por primera vez a un parqueadero.
        /// - Se valida que el parqueadero exista.
        /// </remarks>
        [HttpGet("vehprimeravez")]
        [Authorize(Roles = "ADMIN,SOCIO")]
        public async Task<IActionResult> GetVehPrimeraVez(int parqueaderoId)
        {
            string error = await ValidarParqueadero(parqueaderoId);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });
            
            List<Vehiculo> vehiculos = new IndicadoresService(_db).GetVehPrimeraVez(parqueaderoId);
            return Ok(vehiculos);
        }

        /// <summary>
        /// Obtiene las ganancias de un parqueadero en rango actual DIARIO (1), SEMANAL(7), MENSUAL(30), ANUAL(365).
        /// </summary>
        /// <param name="parq">Información del parqueadero y el rango de días.</param>
        /// <returns>Ingresos generados por el parqueadero en el rango especificado.</returns>
        /// <response code="200">Devuelve las ganancias del parqueadero (IngresosParqueadero)</response>
        /// <response code="400">Error al validar el parqueadero (mensaje)</response>
        /// <remarks>
        /// - Este endpoint permite a usuarios con rol **SOCIO** consultar las ganancias de su parqueadero.
        /// - Se calcula automáticamente el rango actual de fechas basado en los días proporcionados.
        /// - Se valida que el parqueadero exista.
        /// </remarks>
        [HttpGet("GananciasByParquedero")]
        [Authorize(Roles = "SOCIO")]
        public async Task<IActionResult> GetGananciasByParqueadero(ParqueaderoRangoVM parq)
        {
            string error = await ValidarParqueadero(parq.ParqueaderoId);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            (DateTime fechaIni, DateTime fechaFin) = FechaUtils.CalcularFechas(parq.RangoDias);
            Parqueadero parqueadero = await _db.Parqueadero.FindAsync(parq.ParqueaderoId);
            
            IngresosParqueadero ingresos = new IndicadoresService(_db).GetGananciasByParqueadero(parqueadero, fechaIni, fechaFin);
            return Ok(ingresos);
        }

        /// <summary>
        /// Obtiene los 3 socios con mayores ingresos en la última semana.
        /// </summary>
        /// <returns>Lista de los 3 socios con mayores ingresos.</returns>
        /// <response code="200">Lista de los 3 socios con mayores ingresos (List IngresosUser)</response>
        /// <remarks>
        /// - Este endpoint permite a usuarios con rol **ADMIN** consultar los 3 socios con mayores ingresos en la última semana.
        /// - El rango de fechas es predeterminado a semanal.
        /// </remarks>
        [HttpGet("TopGanaciasBySocio")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetTopGanaciasBySocio()
        {
            (DateTime fechaIni, DateTime fechaFin) = FechaUtils.CalcularFechas(RangoDias.SEMANAL);

            List<IngresosUser> topIngresosUser = new IndicadoresService(_db).GetTopGanaciasBySocio(fechaIni, fechaFin, 3);                
            return Ok(topIngresosUser);
        }

        /// <summary>
        /// Obtiene los 3 parqueaderos con mayores ingresos en la última semana.
        /// </summary>
        /// <returns>Lista de los 3 parqueaderos con mayores ingresos.</returns>
        /// <response code="200">Lista de los 3 parqueaderos con mayores ingresos (List IngresosParqueadero)</response>
        /// <remarks>
        /// - Este endpoint permite a usuarios con rol **ADMIN** consultar los 3 parqueaderos con mayores ingresos en la última semana.
        /// - El rango de fechas es predeterminado a semanal.
        /// </remarks>
        [HttpGet("TopGanaciasByParq")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetTopGanaciasByParq()
        {
            (DateTime fechaIni, DateTime fechaFin) = FechaUtils.CalcularFechas(RangoDias.SEMANAL);

            List<IngresosParqueadero> topIngresosParq = new IndicadoresService(_db).GetTopGanaciasByParq(fechaIni, fechaFin, 3);
            return Ok(topIngresosParq);
        }
        #endregion

        #region Metodos Auxiliares
        private async Task<string> ValidarParqueadero(int parqueaderoId)
        {
            var user_role = User.FindFirst(ClaimTypes.Role)?.Value;
            var parqueadero = await _db.Parqueadero.FindAsync(parqueaderoId);

            if (user_role == "SOCIO")
            {
                string socio_id = User.FindFirst(ClaimTypes.Actor)?.Value;

                if (!parqueadero.SocioId.Equals(socio_id))
                    return "No puede ver detalles, parqueadero no asociado a su cuenta";
            }

            if (parqueadero == null)
                return "Parqueadero no existe";

            return string.Empty;
        }
        #endregion
    }
}
