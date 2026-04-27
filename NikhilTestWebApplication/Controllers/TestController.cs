using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Concurrent;

namespace NikhilTestWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private static readonly ConcurrentDictionary<string, string> _users = new();
        
        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public class SignUpModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class SignInModel
        {
            public string UserName { get; set; }    
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SignUpAPI([FromBody] SignUpModel model)
        {
            //try
            //{
            //    if (string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            //    {
            //        return BadRequest("Username and password are required");
            //    }
            //    if (_users.ContainsKey(model.UserName))
            //        return Conflict("user already exists");

            //    _users[model.UserName] = model.Password;

            //    return Ok(new { message = "User registered successfully..." });

            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, ex.Message);
            //}
            

            if (_users.ContainsKey(model.UserName))
                return Conflict("User already exists.");

            _users[model.UserName] = model.Password;
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> SignInAPI([FromBody] SignInModel model)
        {
            //try
            //{
            //    if(string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            //    return BadRequest("Username and password are required");

            //    if (!_users.TryGetValue(model.UserName, out var storedPassword))
            //    return Unauthorized("Invalid username or password");

            //    if(storedPassword != model.Password)
            //    return Unauthorized("Invalid username or password");

            //    return Ok(new { message = "Login Successful", user = model.UserName });

            //}
            //catch (Exception ex) { 
            //    return StatusCode(500, ex.Message);

            //}
            if (!_users.TryGetValue(model.UserName, out var storedPassword) || storedPassword != model.Password)
                return Unauthorized("Invalid username or password.");

            var token = GenerateJwtToken(model.UserName);
            return Ok(new { message = "Login successful", token });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var credentils = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentils
             );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetUserProfile()
        {
            var username = User.Identity?.Name ?? "Unknown";
            return Ok(new { message = $"Welcome {username}! This is a protected route" });
        }
    }
}
