using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserProfileApp.Helpers
{
    /// <summary>
    /// Custom attribute for password validation, enforcing rules such as length, uppercase, lowercase, and digit requirements.
    /// </summary>
    public class CustomPasswordAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates the password based on custom criteria such as minimum length, uppercase, lowercase, and digit presence.
        /// </summary>
        /// <param name="value">The password value to validate.</param>
        /// <param name="validationContext">The context in which the validation is performed.</param>
        /// <returns>A <see cref="ValidationResult"/> object indicating success or failure of the validation.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Password is required.");
            }

            // Validate minimum length
            if (password.Length < 12)
            {
                return new ValidationResult("Password must be at least 12 characters long.");
            }

            // Validate at least one uppercase letter
            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            // Validate at least one lowercase letter
            if (!Regex.IsMatch(password, "[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            // Validate at least one digit
            if (!Regex.IsMatch(password, "[0-9]"))
            {
                return new ValidationResult("Password must contain at least one number.");
            }

            return ValidationResult.Success;
        }
    }
}
