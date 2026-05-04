using Microsoft.AspNetCore.Http.HttpResults;
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
            var users = await _userService.GetAll();

            var response = users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _userService.GetById(id);

            if (user == null)
                return NotFound();

            var response = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
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

            var response = new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                Role = created.Role 
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserRequestDto request)
        {
            var user = await _userService.GetById(id);
            
            if(user == null) return NotFound();

            if (request.Username != null)
                user.Username = request.Username;

            if (request.Email != null)
                user.Email = request.Email;

            if (request.Role != null)
                user.Role = request.Role;

            var updated = await _userService.Update(user);

            var response = new UserDto
            {
                Id = updated.Id,
                Username = updated.Username,
                Email = updated.Email,
                Role = updated.Role
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.Delete(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> RestoreUser(Guid id) {
            var user = await _userService.RestoreUser(id);
            if (!user) return NotFound();
            return Ok(new { message = "user restored." });
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

        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            var fileBytes = await _userService.ExportUsers();

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Users_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
    }
}
