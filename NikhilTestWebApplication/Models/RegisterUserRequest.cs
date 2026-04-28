namespace NikhilTestWebApplication.Models
{
    public class RegisterUserRequest
    {
        public string email {  get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public int age { get; set; }
    }
}
