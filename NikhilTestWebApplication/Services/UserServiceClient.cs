using NikhilTestWebApplication.Models;
using System.Net.Http.Json;

namespace NikhilTestWebApplication.Services
{
    public class UserServiceClient
    {
        private readonly HttpClient _http;

        public UserServiceClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("UserService");
        }

        public async Task<UserDto?> GetUserById(Guid id)
        {
            return await _http.GetFromJsonAsync<UserDto>($"api/User/Get/{id}");
        }
    }
}