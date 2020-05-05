using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("login")]
        public async Task<ActionResult> Authenticate([FromBody]User userParam)
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
        public async Task<ActionResult> Register([FromBody]User userParam)
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

        [Authorize]
        [HttpPost("register_firebase_token")]
        public async Task<ActionResult> RegisterFirbaseToken(string token)
        {
            try
            {
                string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _userService.RegisterFirebaseToken(currentUserId, token);
                _logger.LogInformation($"Registred firebase token (Id = {currentUserId})");
                return StatusCode(201);
            }
            catch (ArgumentNullException)
            {
                _logger.LogError($"Can't register firebase token. User not found.");
                return BadRequest();
            }
        }
    }
}