using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lr2_Kobzeva.Models;
using Microsoft.AspNetCore.Authorization;

namespace Lr2_Kobzeva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Получить всех авторов и их книги
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();

            return Ok(authors);
        }

        // GET: Получить автора по ID
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound("Автор не найден.");

            return Ok(author);
        }
    }
}
