using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtMySql.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int isActive { get; set; } = 1; // 1 for active, 0 for inactive      
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;      
    }

   
}