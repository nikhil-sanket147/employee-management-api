using System.ComponentModel.DataAnnotations.Schema;

namespace NikhilTestWebApplication.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}