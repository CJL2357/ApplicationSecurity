using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "First Name is required.")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "NRIC is required.")]
        [DataType(DataType.Text)]
        public string NRIC { get; set; } // This will be encrypted later

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StrongPassword] // Custom validation attribute for strong password
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Resume is required.")]
        [DataType(DataType.Upload)]
        public IFormFile Resume { get; set; } // For file uploads

        [Required(ErrorMessage = "Who Am I is required.")]
        [DataType(DataType.MultilineText)]
        public string WhoAmI { get; set; }
    }

    // Custom validation attribute for strong password
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password) || !IsStrongPassword(password))
            {
                return new ValidationResult("Password must be at least 12 characters long and include a mix of upper-case, lower-case, numbers, and special characters.");
            }

            return ValidationResult.Success;
        }

        private bool IsStrongPassword(string password)
        {
            var strongPasswordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$");
            return strongPasswordPattern.IsMatch(password);
        }
    }
}