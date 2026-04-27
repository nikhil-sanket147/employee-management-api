using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Interfaces
{
    public interface IUserClient
    {
        Task<List<UserDto>> GetUsers();
    }

}
