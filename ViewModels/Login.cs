using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class Login
    {
        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage="Password is required.")][DataType(DataType.Password)] public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
