using System.ComponentModel.DataAnnotations;

namespace DShop2024.Repository.Validation
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;

        public AllowedExtensionsAttribute(string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_allowedExtensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
