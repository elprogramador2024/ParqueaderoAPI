using EmailModel.Comunes;
using EmailModel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.ViewModels.RegistroVehiculo;
using ParqueaderoAPI.Services;
using ParqueaderoAPI.Services.Comunes;
using System.Net;
using System.Security.Claims;

namespace ParqueaderoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistroVehiculoController : ControllerBase
    {
        #region Propiedades
        private readonly ApiClient _apiClient;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly string _urlEmailApi;
        #endregion

        #region Constructores
        public RegistroVehiculoController(ApiClient apiClient, ApplicationDbContext db, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _db = db;
            _configuration = configuration;
            _urlEmailApi = _configuration["EmailApi:Url"];
        }
        #endregion

        #region Metodos
        /// <summary>
        /// Registra el ingreso o salida de un vehículo en un parqueadero.
        /// </summary>
        /// <param name="model">Información del vehículo y tipo de registro (INGRESO/SALIDA).</param>
        /// <returns>Resultado del registro del vehículo.</returns>
        /// - Este endpoint permite registrar el ingreso o salida de un vehículo en un parqueadero.
        /// <response code="201">Registro ingresado/actualizado exitosamente! y mensaje de EmailAPI (retorna mensaje y Vehiculo/RegistroVehiculo)</response>
        /// <response code="400">Error en la validación de los datos (mensaje)</response>
        /// <remarks>
        /// - El rol necesario para acceder a este endpoint es **SOCIO**.
        /// - En el caso de ingreso, se registra la fecha de ingreso y el ID del parqueadero en el vehículo.  
        /// - En el caso de salida, se registra la fecha de salida en registroVehiculo y se actualiza el estado del vehículo.
        /// </remarks>
        [HttpPost("registrar")]
        [Authorize(Roles = "SOCIO")]
        public async Task<IActionResult> Registrar([FromBody] RegistroVehiculoVM model)
        {
            (string error, Vehiculo vehiculo, EmailVM email) = await ValidarRegistro(model);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            if (model.TipoRegistro == TipoRegistro.INGRESO)
            {
                vehiculo.Placa = model.Placa;
                vehiculo.FechaIngreso = DateTime.Now;
                vehiculo.ParqueaderoId = model.ParqueaderoId;

                var result = new VehiculoService(_db).Upsert(vehiculo);

                if (result > 0)
                {                    
                    Msj Msj = await _apiClient.PostAsync($"{_urlEmailApi}/Email/enviar", email);
                    return StatusCode((int) HttpStatusCode.Created, new { mensaje = $"Registro ingresado exitosamente! {Msj.Mensaje}", vehiculo = vehiculo });
                }
            }

            if (model.TipoRegistro == TipoRegistro.SALIDA)
            {
                RegistroVehiculo registroVehiculo = new RegistroVehiculo()
                {
                    FechaIngreso = (DateTime)vehiculo.FechaIngreso,
                    FechaSalida = DateTime.Now,
                    ParqueaderoId = vehiculo.ParqueaderoId,
                    VehiculoId = vehiculo.Id
                };
                
                var result = new RegistroVehiculoService(_db).Insert(registroVehiculo);

                if (result > 0)
                {
                    vehiculo.ParqueaderoId = 0;
                    vehiculo.FechaIngreso = null;

                    result = new VehiculoService(_db).Upsert(vehiculo);
                    //result = _db.SaveChanges();

                    if (result > 0)
                    {
                        Msj Msj = await _apiClient.PostAsync($"{_urlEmailApi}/Email/enviar", email);
                        return StatusCode((int)HttpStatusCode.Created, new { mensaje = $"Registro actualizado exitosamente! {Msj.Mensaje}", registro = registroVehiculo });
                    }
                }
            }

            return BadRequest(new { mensaje = $"No fue posible {(model.TipoRegistro == TipoRegistro.INGRESO ? "ingresar" : "actualizar")} registro" });

        }
        #endregion

        #region Metodos
        private async Task<(string, Vehiculo, EmailVM)> ValidarRegistro(RegistroVehiculoVM model)
        {
            var parqueadero = await _db.Parqueadero.FindAsync(model.ParqueaderoId);
            if (parqueadero == null)
                return ("Parqueadero no existe", null, null);

            string socio_id = User.FindFirst(ClaimTypes.Actor)?.Value;

            if (parqueadero.SocioId != socio_id)
                return ("Parqueadero no asociado a su cuenta", null, null);

            var vehiculos = _db.Vehiculo.Where((v) => v.Placa == model.Placa).ToList();
            
            Vehiculo vehiculo = vehiculos.Any() ? vehiculos.FirstOrDefault() : new Vehiculo();
            EmailVM email;

            if (model.TipoRegistro == TipoRegistro.INGRESO)
            {
                if (vehiculo != null)
                {
                    if (vehiculo.FechaIngreso != null && vehiculo.ParqueaderoId != 0)
                        return ("No se puede Registrar Ingreso, ya existe la placa en este u otro parqueadero", null, null);
                }

                var vehiculosParq = _db.Vehiculo.Where((v) => v.ParqueaderoId == model.ParqueaderoId).ToList();
                if (vehiculosParq.Count() >= parqueadero.Capacidad)
                    return ($"No se puede Registrar Ingreso, parqueadero excede su capacidad de {parqueadero.Capacidad} sitios", null, null);
            }

            if (model.TipoRegistro == TipoRegistro.SALIDA)
            {
                if ((vehiculo?.FechaIngreso == null && vehiculo?.ParqueaderoId == 0) || vehiculo?.ParqueaderoId != model.ParqueaderoId)
                    return ("No se puede Registrar Salida, no existe la placa en el parqueadero", null, null);
            }

            email = new EmailVM()
            {
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                ParqueaderoNombre = parqueadero.Nombre,
                Placa = model.Placa,
                Mensaje = $"Estimado usuario, Le informamos que el vehículo con placa {vehiculo.Placa} " +
                          $"ha registrado su {model.TipoRegistro} en el parqueadero {parqueadero.Nombre}. " +
                          $"Gracias por utilizar nuestros servicios"
            };

            return (string.Empty, vehiculo, email);
        }

        #endregion
    }
}
