using ParqueaderoAPI.Models.ViewModels.Indicadores;
using System.ComponentModel.DataAnnotations;

namespace ParqueaderoAPI.Utility
{
    public class ValidEnum<T> : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is T rango)
            {
                if (Enum.IsDefined(typeof(T), rango))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult($"El valor seleccionado no es válido. Debe ser {string.Join(", ", Enum. GetValues(typeof(T)).Cast<T>().Select(e => $"{e} ({Convert.ToInt32(e)})") )}");
        }
    }
}
