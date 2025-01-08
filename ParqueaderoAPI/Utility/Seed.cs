using Microsoft.AspNetCore.Identity;
using ParqueaderoAPI.Models;
using ParqueaderoAPI.Models.ViewModels.Usuario;

namespace ParqueaderoAPI.Utility
{
    public class Seed
    {
        public static string[] Roles = { "ADMIN", "SOCIO" };

        public static async Task Inicializar(IServiceScope scope)
        {
            //Inicializamos Roles y Usuario admin
            using (scope)
            {
                try
                {
                    var roleManager = (RoleManager<IdentityRole>)scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>));
                    var userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>));

                    await SeedRolesAsync(roleManager);
                    await SeedUsersAsync(userManager);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al inicializar roles: {ex.Message}");
                }

            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var rol in Roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            List<RegistroUsuarioVM> users = new List<RegistroUsuarioVM>()
            {
                new ()
                {
                    Nombre = "admin",
                    Email = "admin@mail.com",
                    Password = "admin",
                    ConfirmPassword = "admin",
                    Rol = "ADMIN"
                }
            };

            foreach (var u in users)
            {
                var user = new ApplicationUser { UserName = u.Nombre, Email = u.Email };
                var result = await userManager.CreateAsync(user, u.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.Rol);
                }
            }
        }
    }
}
