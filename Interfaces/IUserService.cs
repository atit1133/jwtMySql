using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtMySql.Models;

namespace JwtMySql.Interfaces
{
    public interface IUserService
    {
        User? Authenticate(string username, string password);
        IEnumerable<User> GetAllUsers();
        User? GetUserById(int id);
        User? Register(User user);
        String? GenerateJwtToken(User user);
        bool UpdateUser(User user);
        bool DeleteUser(int id);
    }
}