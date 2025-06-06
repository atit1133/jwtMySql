using Microsoft.AspNetCore.Mvc;
using JwtMySql.Models;
//use 3 packages for JWT
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
//----------
using System.Text;

namespace JwtMySql.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class LoginController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public LoginController(MyDbContext context, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Context cannot be null.");
            }
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));        
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.PasswordHash == user.PasswordHash);
            if (existingUser == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Here you would typically generate a JWT token and return it
            // For simplicity, we are just returning a success message
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, existingUser.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token (JWT ID)
                new Claim("UserId", existingUser.UserId.ToString())
            };

            //call the secret key from appsettings.json
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, "JWT key is not configured.");
            }
            //create a symmetric security key using the JWT key
            //this key should be kept secret and not exposed in the client-side code
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            //create the signing credentials
            //using HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token expiration time
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);


            return Ok(new { token = tokenString , userId = existingUser.UserId, username = existingUser.Username });
        }
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            Console.WriteLine("Registering : ", user);
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("Invalid user data.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }
            var newUser = new User
            {
                Username = user.Username,
                PasswordHash = user.PasswordHash, // In a real application, hash the password
                Email = user.Email,
                isActive = 1, // Set to active by default
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.UserId }, newUser);
        }

    }
}