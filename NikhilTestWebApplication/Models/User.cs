namespace NikhilTestWebApplication.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public bool IsArchieved { get; set; } = false;
    }
}
