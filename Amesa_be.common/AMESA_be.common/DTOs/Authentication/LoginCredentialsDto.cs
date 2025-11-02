using System.ComponentModel.DataAnnotations;

namespace AMESA_be.common.DTOs.Authentication
{
    public class LoginCredentialsDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Pass { get; set; }
    }
}
