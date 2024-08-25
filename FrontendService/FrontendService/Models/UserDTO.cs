namespace FrontendService.Models
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
    }
}
