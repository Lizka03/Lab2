using Lr2_Kobzeva.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lr2_Kobzeva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Получить всех пользователей
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: Получить пользователя по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден.");
            return Ok(user);
        }

        // POST: Добавить пользователя
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Пользователь зарегистрирован.");
        }

        // PUT: Обновить данные пользователя по ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден.");

            user.Username = updatedUser.Username ?? user.Username;
            user.Email = updatedUser.Email ?? user.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok($"Пользователь {id} обновлён.");
        }

        // DELETE: Удалить пользователя по ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok($"Пользователь {id} удалён.");
        }
    }
}
