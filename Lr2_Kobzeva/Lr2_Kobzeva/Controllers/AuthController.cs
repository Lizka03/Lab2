using Lr2_Kobzeva.Models;
using Lr2_Kobzeva;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }
    public struct LoginData
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
    // Регистрация нового пользователя
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        // Проверяем, существует ли пользователь с таким логином
        if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            return BadRequest("Пользователь с таким логином уже существует.");

        // Проверка: если флаг IsAdmin установлен
        //if (user.IsAdmin)
        //{
            // Проверяем, авторизован ли текущий пользователь и является ли он администратором
          //  if (!User.Identity.IsAuthenticated || !_context.Users.Any(u => u.Login == User.Identity.Name && u.IsAdmin))
            //    return Forbid("Вы не можете зарегистрировать администратора.");
        //}

        // Добавляем пользователя в базу
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Пользователь успешно зарегистрирован.");
    }


    // Авторизация пользователя
    [HttpPost("login")]
    public IActionResult GetToken([FromBody] LoginData ld)
    {
        if (string.IsNullOrEmpty(ld.Login) || string.IsNullOrEmpty(ld.Password))
            return BadRequest(new { message = "Логин и пароль обязательны" });

        // Хэшируем введённый пароль для проверки
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hashedPassword = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ld.Password)))
                .Replace("-", "").ToLower();

            // Ищем пользователя в базе данных
            var user = _context.Users.FirstOrDefault(u =>
                u.Login == ld.Login && u.Password == hashedPassword);

            if (user == null)
            {
                // Если пользователь не найден, возвращаем 401 Unauthorized
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            // Генерируем токен для пользователя
            var token = AuthOptions.GenerateToken(user.Username, user.IsAdmin);

            // Возвращаем токен
            return Ok(new { Token = token, Message = "Авторизация успешна" });
        }
    }
}



