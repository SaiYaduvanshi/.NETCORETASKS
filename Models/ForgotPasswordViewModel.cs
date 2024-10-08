using System.ComponentModel.DataAnnotations;

namespace UserProfileApp.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
