using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using JwtMySql.Interfaces;
using JwtMySql.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JwtMySql.Services
{
    public class UserService : IUserService
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public UserService(MyDbContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
        }
        public User? Authenticate(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Authentication failed
            }
            return user;
        }

        public IEnumerable<User> GetAllUsers()
        {
            var users = _context.Users.ToList();
            if (users == null || !users.Any())
            {
                throw new InvalidOperationException("No users found.");
            }
            return users;
        }

        public User? GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return user;
        }

        public User Register(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(user.Username));
            }
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(user.PasswordHash));
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with username {user.Username} already exists.");
            }
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            var newUser = new User
            {
                Username = user.Username,
                PasswordHash = newPasswordHash,
                Email = user.Email,
                isActive = 1, // Default to active
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return newUser;
        } 

        public bool DeleteUser(int id)
        {
            throw new NotImplementedException();
        }

        public string GenerateJwtToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //Unique identifier for the JWT (JWT ID)
                new Claim("UserId", user.UserId.ToString())
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Token expiration time
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}