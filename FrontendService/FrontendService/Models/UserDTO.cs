using System.ComponentModel.DataAnnotations;

namespace FrontendService.Models
{
    public class UserDTO
    {
        public int UserId { get; set; }
        [Display(Name = "User Name")]
        public required string UserName { get; set; }
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
    }
}
