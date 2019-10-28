using System.ComponentModel.DataAnnotations;

namespace FileShare.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 4, ErrorMessage = "You must specify a password between 4 and 255 characters")]
        public string Password { get; set; }
    }
}
