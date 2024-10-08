using System.ComponentModel.DataAnnotations;
using UserProfileApp.Helpers;

namespace UserProfileApp.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [CustomPassword(ErrorMessage = "Password must be at least 12 characters long, include at least one uppercase letter, one lowercase letter, and one number.")]

        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [CustomPassword(ErrorMessage = "Password must be at least 12 characters long, include at least one uppercase letter, one lowercase letter, and one number.")]

        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}
