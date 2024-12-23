using Lr2_Kobzeva.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить всех пользователей (только админ)
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.Login,
            IsAdmin = u.IsAdmin,
            u.RentedBookIds
        }));
    }
    [HttpGet("{id}")]
    [Authorize] // Только авторизованные пользователи могут выполнять этот запрос
    public async Task<IActionResult> GetUserById(int id)
    {
        // Найти пользователя по ID
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден." });
        }

        // Возвращаем данные о пользователе (без пароля)
        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            login = user.Login,
            isAdmin = user.IsAdmin,
            rentedBookIds = user.RentedBookIds
        });
    }

    // Добавление нового пользователя (только админ)
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

        if (currentUser == null || !currentUser.IsAdmin)
            return Forbid("У вас недостаточно прав для добавления пользователя.");

        if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            return BadRequest("Пользователь с таким логином уже существует.");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Пользователь успешно добавлен.");
    }

    // Удаление пользователя (только админ)
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

        if (currentUser == null || !currentUser.IsAdmin)
            return Forbid("У вас недостаточно прав для удаления пользователя.");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound("Пользователь не найден.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok("Пользователь успешно удалён.");
    }
    [HttpGet("users-with-rented-books")]
    public IActionResult GetUsersWithRentedBooks()
    {
        var usersWithBooks = _context.Users
            .Where(u => u.RentedBookIds.Any())
            .Select(u => new
            {
                Username = u.Username,
                RentedBooks = _context.Books
                    .Where(b => u.RentedBookIds.Contains(b.Id))
                    .Select(b => b.Title)
                    .ToList()
            })
            .ToList();

        return Ok(usersWithBooks);
    }
    [HttpPut("{id}")]
    [Authorize] // Только авторизованные пользователи могут изменять свои данные
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
    {
        // Найти пользователя в базе данных
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден." });
        }

        // Проверяем, что пользователь пытается изменить только свои данные или он администратор
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
        if (currentUser == null || (currentUser.Id != id && !currentUser.IsAdmin))
        {
            return Forbid("У вас нет прав для изменения этого пользователя.");
        }

        // Обновляем данные
        user.Username = updatedUser.Username ?? user.Username;
        user.Email = updatedUser.Email ?? user.Email;
        user.Login = updatedUser.Login ?? user.Login;

        // Хэшируем новый пароль, если он указан
        if (!string.IsNullOrEmpty(updatedUser.Password))
        {
            user.Password = updatedUser.Password; // Используется свойство с хэшированием
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Данные пользователя успешно обновлены.",
            user = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Login,
                user.IsAdmin
            }
        });
    }


    [HttpPost("reset-identity")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ResetIdentity()
    {
        await _context.ResetIdentity("Users");
        return Ok("IDENTITY успешно сброшен.");
    }

}
