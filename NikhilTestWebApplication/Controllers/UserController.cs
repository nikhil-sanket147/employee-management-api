using Microsoft.AspNetCore.Mvc;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userService.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateUserRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            var created = await _userService.Add(user);

            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            var updated = await _userService.Update(id, user);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.Delete(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted" });
        }

        [HttpPost]
        public async Task<UploadFileModel> UploadFile([FromForm] UploadFile uploadFile)
        {
            var response = await _userService.UploadFile(uploadFile);
            return response;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination)
        {
            var result = await _userService.GetUsersAsync(pagination);
            return Ok(result);
        }
    }
}
