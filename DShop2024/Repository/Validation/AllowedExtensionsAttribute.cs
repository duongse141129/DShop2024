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
                    return new ValidationResult(ErrorMessage ?? $"File extension '{extension}' is not allowed. Allowed extensions are: {string.Join(", ", _allowedExtensions)}.");
                }
            }
            return ValidationResult.Success;
        }
        //[AllowedExtensions(new string[] { ".jpg", ".png", ".gif" }, ErrorMessage = "Only JPG, PNG, and GIF files are allowed.")]
    }
}
