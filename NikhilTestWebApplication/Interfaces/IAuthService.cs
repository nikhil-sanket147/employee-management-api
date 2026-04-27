using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Interfaces
{
    public interface IAuthService
    {
        string? Login(LoginRequest request);
    }
}
