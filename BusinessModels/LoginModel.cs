using System.ComponentModel.DataAnnotations;

namespace Cipa.BusinessModels
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}