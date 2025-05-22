using APISTEAMSTATS.Dtos;
using APISTEAMSTATS.models;
using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Mvc;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("auth")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> loginUser(User user)
        {
            return Ok(await _userService.Register(user));
        }

        [HttpPost("register")]
        public async Task<IActionResult> registerUser(User user) // Usa o DTO de input
        {
            var newUser = await _userService.Register(user);

            if (newUser == null)
            {
                return Conflict("Usuário com esse login já existe!");
            }

            UserResponseDto responseDto = new UserResponseDto
            {
                Id = newUser.Id,
                Name = newUser.name,
                Login = newUser.login
            };

            return CreatedAtAction(nameof(registerUser), new { id = newUser.Id }, responseDto);
        }
    }
}