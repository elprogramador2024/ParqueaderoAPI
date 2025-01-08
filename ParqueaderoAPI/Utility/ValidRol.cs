using System.ComponentModel.DataAnnotations;

namespace ParqueaderoAPI.Utility
{
    public class ValidRol : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("El campo rol es requerido");
            }

            string rol = value.ToString().ToUpper();

            if (Array.Exists(Seed.Roles, r => r == rol))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"Debe asignar solo roles válidos: {string.Join(", ", Seed.Roles)}");
        }
    }
}
