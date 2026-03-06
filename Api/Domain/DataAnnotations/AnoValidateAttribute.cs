using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.DataAnnotations
{
    public class AnoValidateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int ano) return ano >= 1900 && ano <= DateTime.Now.Year;            
            
            return false;
        }
    }
}
