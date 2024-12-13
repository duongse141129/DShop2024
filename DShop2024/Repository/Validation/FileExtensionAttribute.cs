using System.ComponentModel.DataAnnotations;

namespace DShop2024.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is IFormFile fille)
            {
                var extension = Path.GetExtension(fille.FileName);

                string[] extensions = { "jpg", "png", "jpeg" };

                bool result = extension.Any(x => extension.EndsWith(x));

                if(!result)
                {
                    return new ValidationResult("Allowed extensions are jpg or png or jpeg");
                }
            }
            return ValidationResult.Success;
        }


    }
}
