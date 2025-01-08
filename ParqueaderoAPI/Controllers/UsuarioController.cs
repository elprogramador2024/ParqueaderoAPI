using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.ViewModels.Usuario;
using ParqueaderoAPI.Services.Comunes;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ParqueaderoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        #region Propiedades
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructores
        public UsuarioController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        #endregion

        #region Metodos

        /// <summary>
        /// Obtiene todos los usuarios registrados.
        /// </summary>
        /// <returns>Una lista de usuarios con sus roles.</returns>
        /// <response code="200">Lista de Usuarios (List UsuarioVM)</response>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var asp_users = _userManager.Users.ToList();

            var users = new List<UsuarioVM>();

            foreach (var u in asp_users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                users.Add(new UsuarioVM
                {
                    Id = u.Id,
                    Nombre = u.UserName,
                    Email = u.Email,
                    Rol = roles.FirstOrDefault()
                });
            }
            return Ok(users);
        }

        /// <summary>
        /// Inicia sesión de usuario.
        /// </summary>
        /// <param name="model">Credenciales de inicio de sesión del usuario.</param>
        /// <returns>Token JWT si el inicio de sesión es exitoso.</returns>
        /// <response code="200">Inicio de sesión exitoso (retorna token y username).</response>
        /// <response code="401">Usuario no vállido. Credenciales incorrectas. (mensaje)</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtToken(user);

                return Ok(new { token, user.UserName });
            }

            return Unauthorized(new { mensaje = "Usuario no válido! Credenciales incorrectas." }); 
        }

        /// <summary>
        /// Cierra la sesión del usuario y agrega el token a la lista negra.
        /// </summary>
        /// <param name="authorization">Token JWT del usuario en el encabezado.</param>
        /// <param name="tokenBlacklistService">Servicio para agregar el token a la lista negra.</param>
        /// <returns>Confirmación de cierre de sesión.</returns>
        /// <response code="200">Ha cerrado sesión correctamente! (mensaje)</response>
        /// <response code="400">El Token es requerido (mensaje)</response>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader] string authorization, [FromServices] ITokenBlackListService tokenBlacklistService)
        {
            if (string.IsNullOrEmpty(authorization))
                return BadRequest(new { mensaje = "El Token es requerido" });

            var token = authorization.Replace("Bearer ", "");

            DateTime tokenExpira = ExtractJwtExpira(token);
            tokenBlacklistService.AddToBlacklist(token, tokenExpira);
            
            return Ok(new { mensaje = "Ha cerrado sesión correctamente!" });
        }

        /// <summary>
        /// Inserta un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="model">Datos del usuario a registrar.</param>
        /// <returns>Confirmación del usuario creado.</returns>
        /// <response code="201">Usuario creado exitosamente! (retorna mensaje y UsuarioVM)</response>
        /// <response code="400">Error en la validación de los datos (mensaje)</response>
        [HttpPost("insert")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Insert([FromBody] RegistroUsuarioVM model)
        {
            string error = await ValidarRol(model.Rol);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            var user = new ApplicationUser { UserName = model.Nombre, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Rol);
                UsuarioVM usuario = new UsuarioVM()
                {
                    Id = user.Id,
                    Nombre = user.UserName,
                    Email = user.Email,
                    Rol = model.Rol
                };
                return StatusCode((int)HttpStatusCode.Created, new { mensaje = "Usuario creado exitosamente!", usuario = usuario });
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="model">Datos del usuario a actualizar.</param>
        /// <returns>Confirmación del usuario actualizado.</returns>
        /// <response code="200">Usuario actualizado exitosamente! (retorna mensaje y UsuarioVM)</response>
        /// <response code="400">Error en la validación de los datos (mensaje)</response>
        [HttpPut("update")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update([FromBody] UsuarioVM model)
        {
            string error = await ValidarRol(model.Rol);
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { mensaje = error });

            var user = await _userManager.FindByIdAsync(model.Id);
            user.Email = model.Email;
            user.UserName = model.Nombre;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Rol);
                UsuarioVM usuario = new UsuarioVM()
                {
                    Id = user.Id,
                    Nombre = user.UserName,
                    Email = user.Email,
                    Rol = model.Rol
                };
                return Ok(new { mensaje = "Usuario actualizado exitosamente!", usuario = usuario });
            }

            return BadRequest(new { mensaje = result.Errors });
        }

        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="model">Datos del usuario a eliminar.</param>
        /// <returns>Confirmación de la eliminación.</returns>
        /// <response code="200">Usuario eliminado exitosamente! (mensaje)</response>
        /// <response code="400">Error al intentar eliminar el usuario (mensaje)</response>
        [HttpDelete("delete")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete([FromBody] UsuarioVM model)
        {
            var user = await _userManager.FindByNameAsync(model.Nombre);
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { mensaje = "Usuario eliminado exitosamente!" });
            }

            return BadRequest(new { mensaje = result.Errors });
        }
        #endregion

        #region Metodos Auxiliares
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Actor, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(int.Parse(_configuration["Jwt:ExpiraHoras"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private DateTime ExtractJwtExpira(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;

            return expiry;
        }

        private async Task<string> ValidarRol(string rol)
        {
            //if (rol != "SOCIO")
            //    return "Rol no válido";

            if (!await _roleManager.RoleExistsAsync(rol))
                return "Rol no existe";

            return string.Empty;
        }
        #endregion








    }
}
