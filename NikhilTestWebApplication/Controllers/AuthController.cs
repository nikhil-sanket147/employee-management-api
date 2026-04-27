using Microsoft.AspNetCore.Mvc;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login(
            LoginRequest request)
        {
            var token =
                _authService.Login(request);

            if (token == null)
                return Unauthorized(
                    "Invalid email or password");

            return Ok(new
            {
                token = token
            });
        }
    }
}