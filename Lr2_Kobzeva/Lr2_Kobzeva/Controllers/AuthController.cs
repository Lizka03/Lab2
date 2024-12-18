using Microsoft.AspNetCore.Mvc;
using Lr2_Kobzeva.Models;

namespace Lr2_Kobzeva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly List<User> _users;

        public AuthController()
        {
            // Инициализация пользователей (вместо базы данных)
            _users = new List<User>
            {
                new User { Username = "admin", PasswordHash = "admin123", is_admin = true },
                new User { Username = "user", PasswordHash = "user123", is_admin = false }
            };
        }

        // POST: /api/Auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginData)
        {
            // Ищем пользователя с указанными именем и паролем
            var user = _users.FirstOrDefault(u =>
                u.Username == loginData.Username && u.PasswordHash == loginData.PasswordHash);

            if (user == null)
            {
                return Unauthorized("Неверное имя пользователя или пароль.");
            }

            // Генерируем токен
            var token = AuthOptions.GenerateToken(user.Username, user.is_admin);
            return Ok(token);
        }
    }
}