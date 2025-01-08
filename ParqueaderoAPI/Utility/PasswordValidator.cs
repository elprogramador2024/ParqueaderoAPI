using Microsoft.AspNetCore.Identity;
using ParqueaderoAPI.Models;

namespace ParqueaderoAPI.Utility
{
    public class PasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            var errors = new List<IdentityError>();

            if (password == "admin")
            {
                return Task.FromResult(IdentityResult.Success);
            }

            // Example: Validate other criteria if needed
            if (password.Length < 5)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "El password debe tener una longitud mínima de 5 carateres "
                });
            }

            return errors.Count == 0
                ? Task.FromResult(IdentityResult.Success)
                : Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
