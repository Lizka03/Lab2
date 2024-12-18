using Lr2_Kobzeva.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lr2_Kobzeva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private static List<Book> _books = new List<Book>();

        // GET: Получить все книги
        [HttpGet]
        public IActionResult GetAllBooks()
        {
            return Ok(_books);
        }

        // GET: Получить доступные книги
        [HttpGet("available")]
        public IActionResult GetAvailableBooks()
        {
            var availableBooks = _books.Where(b => b.IsAvailable).ToList();
            return Ok(availableBooks);
        }

        // POST: Добавить новую книгу
        [HttpPost]
        public IActionResult AddBook([FromBody] Book book)
        {
            book.Id = _books.Count + 1;
            _books.Add(book);
            return Ok("Книга добавлена.");
        }
        // PUT: Обновить данные книги по ID
        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, [FromBody] Book updatedBook)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound("Книга не найдена.");

            book.Title = updatedBook.Title ?? book.Title;
            book.Author = updatedBook.Author ?? book.Author;
            book.IsAvailable = updatedBook.IsAvailable;
            return Ok($"Книга {id} обновлена.");
        }

        // DELETE: Удалить книгу по ID
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound("Книга не найдена.");

            _books.Remove(book);
            return Ok($"Книга {id} удалена.");
        }
    }

}
