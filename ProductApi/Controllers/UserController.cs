using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly ILogger _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]User userParam)
        {
            var user = await _userService.Authenticate(userParam.Email, userParam.Password);

            if (user == null)
            {
                _logger.LogInformation($"Failed to authenticate user {userParam.Email}");
                return BadRequest();
            }

            _logger.LogInformation($"Authenticated user {user.Email}");
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]User userParam)
        {
            try
            {
                var user = await Task.Run(() => _userService.Register(userParam));
                _logger.LogInformation($"Registered user {user.Email}");
                return Ok(user);
            }
            catch (ArgumentException e)
            {
                _logger.LogInformation($"Failed to register new user: {e.Message}");
                return BadRequest();
            }
        }
    }
}