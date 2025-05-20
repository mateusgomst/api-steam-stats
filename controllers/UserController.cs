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
            return Ok("login");
        }

        [HttpPost("register")]
        public async Task<IActionResult> registerUser()
        {
            return Ok("Registrado");
        }
    }
}