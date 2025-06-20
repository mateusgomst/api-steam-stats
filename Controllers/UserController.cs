using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APISTEAMSTATS.Dtos;
using APISTEAMSTATS.models;
using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("auth")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public UserController(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequestDto userDto)
        {
            var user = new User
            {
                Login = userDto.login,
                Password = userDto.password
            };

            var loginUser = await _userService.Login(user);

            if (loginUser == null)
            {
                return Unauthorized("Login ou senha inválidos.");
            }

            var token = _tokenService.GenerateToken(loginUser);


            return Ok(new { token = token });
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
                Name = newUser.Name,
                Login = newUser.Login
            };

            return CreatedAtAction(nameof(registerUser), new { id = newUser.Id }, responseDto);
        }
        
    }
}