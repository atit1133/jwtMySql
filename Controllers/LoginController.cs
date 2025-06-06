using Microsoft.AspNetCore.Mvc;
using JwtMySql.Models;
//use 3 packages for JWT
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
//----------
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using JwtMySql.Interfaces;

namespace JwtMySql.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            var existingUser = _userService.Authenticate(user.Username, user.PasswordHash);
            if (existingUser == null)
                return Unauthorized("Invalid username or password.");

            var token = _userService.GenerateJwtToken(existingUser);
            return Ok(new { token, userId = existingUser.UserId, username = existingUser.Username });
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound("User not found.");
            return Ok(user);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
                return BadRequest("Invalid user data.");

            var existingUser = _userService.Authenticate(user.Username, user.PasswordHash);
            if (existingUser != null)
                return Conflict("Username already exists.");

            var newUser = _userService.Register(user);
            return CreatedAtAction(nameof(GetUser), new { id = newUser.UserId }, newUser);
        }
    }
}