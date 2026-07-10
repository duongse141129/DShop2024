using System.ComponentModel.DataAnnotations;

namespace DShop2024.Repository.Validation 
{ 
    public class VNDPriceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is decimal price && price % 1000 == 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "Price must be divisible by 1000.");
        }

    }
}
