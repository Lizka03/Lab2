using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lr2_Kobzeva.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Lr2_Kobzeva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Получить все книги
        [HttpGet]
        
        public async Task<IActionResult> GetAllBooks()
        {

            var books = await _context.Books.Include(b => b.Authors).ToListAsync();
            return Ok(books);
        }

        // POST: Добавить новую книгу (только для администратора)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (book == null || string.IsNullOrEmpty(book.Title))
                return BadRequest("Название книги не должно быть пустым.");

            // Проверяем, есть ли авторы у книги
            if (book.Authors != null && book.Authors.Any())
            {
                // Создаём список авторов, которые уже существуют в базе или добавляем новых
                var authorsToAttach = new List<Author>();
                foreach (var author in book.Authors)
                {
                    var existingAuthor = await _context.Authors
                        .FirstOrDefaultAsync(a => a.Name == author.Name);
                    if (existingAuthor != null)
                    {
                        authorsToAttach.Add(existingAuthor);
                    }
                    else
                    {
                        authorsToAttach.Add(author);
                        _context.Authors.Add(author);
                    }
                }

                book.Authors = authorsToAttach;
            }
            else
            {
                // Если авторы не указаны, оставляем список пустым
                book.Authors = new List<Author>();
            }
            book.IsAvailable = true; // Книга доступна при добавлении
            book.DateRented = null;  // Дата аренды отсутствует
            book.DueDate = null;     // Дата возврата отсутствует

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AddBook), new { id = book.Id }, new
            {
                book.Id,
                book.Title,
                book.IsAvailable,
                Authors = book.Authors.Select(a => new { a.Id, a.Name }).ToList()
            });
        }


        // DELETE: Удалить книгу (только для администратора)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound("Книга не найдена.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok($"Книга {id} удалена.");
        }
        [HttpGet("books-by-author/{authorName}")]
        public IActionResult GetBooksByAuthor(string authorName)
        {
            var books = _context.Books
                .Where(b => b.Authors.Any(a => a.Name == authorName))
                .Select(b => new
                {
                    b.Title,
                    b.IsAvailable
                })
                .ToList();

            return Ok(books);
        }
        [HttpGet("search-books/{query}")]
        public IActionResult SearchBooks(string query)
        {
            var books = _context.Books
                .Where(b => b.Title.Contains(query))
                .Select(b => new
                {
                    b.Title,
                    b.IsAvailable
                })
                .ToList();

            return Ok(books);
        }
        [HttpPost("{bookId}/rent")]
        [Authorize] // Любой авторизованный пользователь может брать книги
        public async Task<IActionResult> RentBook(int bookId)
        {
            // Найти текущего пользователя по токену
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Пользователь не найден." });
            }

            // Найти книгу по ID
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound(new { message = "Книга не найдена." });
            }

            // Проверить, доступна ли книга
            if (!book.IsAvailable)
            {
                return BadRequest(new { message = $"Книга уже арендована до {book.DueDate:dd.MM.yyyy}." });
            }

            // Связать книгу с текущим пользователем
            book.RentBook(currentUser.Id);

            // Добавить книгу в список арендованных книг у пользователя
            if (!currentUser.RentedBookIds.Contains(bookId))
            {
                currentUser.RentedBookIds.Add(bookId);
            }

            // Сохранить изменения
            _context.Books.Update(book);
            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Книга '{book.Title}' успешно арендована до {book.DueDate:dd.MM.yyyy}.",
                bookId = book.Id,
                rentedByUserId = currentUser.Id,
                dateRented = book.DateRented,
                dueDate = book.DueDate
            });
        }


        [HttpDelete("reset-books")]
        [Authorize(Roles = "admin")] // Только администратор может сбрасывать книги
        public async Task<IActionResult> DeleteAllBooks()
        {
            // Удаляем все записи из таблицы Books
            var books = await _context.Books.ToListAsync();
            _context.Books.RemoveRange(books);
            await _context.SaveChangesAsync();

            // Сбрасываем значение IDENTITY для таблицы Books через _context
            await _context.ResetIdentity("Books");

            return Ok(new { message = "Все книги удалены, идентификаторы сброшены." });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // Только администратор может изменять книги
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book updatedBook)
        {
            // Найти книгу в базе данных
            var book = await _context.Books.Include(b => b.Authors).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound(new { message = "Книга не найдена." });
            }

            // Обновить название и доступность
            book.Title = updatedBook.Title ?? book.Title;
            book.IsAvailable = updatedBook.IsAvailable;

            // Обновить авторов
            if (updatedBook.Authors != null && updatedBook.Authors.Any())
            {
                book.Authors.Clear();
                foreach (var author in updatedBook.Authors)
                {
                    var existingAuthor = await _context.Authors.FindAsync(author.Id);
                    if (existingAuthor == null)
                    {
                        return NotFound(new { message = $"Автор с ID {author.Id} не найден." });
                    }
                    book.Authors.Add(existingAuthor);
                }
            }

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Данные книги успешно обновлены.",
                book = new
                {
                    book.Id,
                    book.Title,
                    book.IsAvailable,
                    authors = book.Authors.Select(a => new { a.Id, a.Name }).ToList()
                }
            });
        }

        [HttpPost("{bookId}/rent-to-user/{userId}")]
        [Authorize(Roles = "admin")] // Только администратор может выполнять эту операцию
        public async Task<IActionResult> AssignBookToUserWithDate(int bookId, int userId, [FromBody] DateTime? dueDate)
        {
            // Найти книгу по ID
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound(new { message = "Книга не найдена." });
            }

            // Проверить, доступна ли книга
            if (!book.IsAvailable)
            {
                return BadRequest(new { message = $"Книга уже арендована до {book.DueDate:dd.MM.yyyy}." });
            }

            // Найти пользователя по ID
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден." });
            }

            // Установить дату возврата (либо вручную, либо по умолчанию +30 дней)
            book.DateRented = DateTime.UtcNow;
            book.DueDate = dueDate ?? DateTime.UtcNow.AddMonths(1);
            book.IsAvailable = false;
            book.RentedByUserId = user.Id;

            // Добавить книгу в список арендованных книг у пользователя
            if (!user.RentedBookIds.Contains(bookId))
            {
                user.RentedBookIds.Add(bookId);
            }

            // Сохранить изменения
            _context.Books.Update(book);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Книга '{book.Title}' успешно арендована пользователем {user.Username} до {book.DueDate:dd.MM.yyyy}.",
                book = new
                {
                    book.Id,
                    book.Title,
                    rentedByUserId = user.Id,
                    dateRented = book.DateRented,
                    dueDate = book.DueDate
                }
            });
        }



        [HttpGet("{id}/status")]
        [Authorize(Roles = "admin")]
        public IActionResult GetBookStatus(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Книга не найдена.");

            return Ok(new
            {
                book.Id,
                book.Title,
                Status = book.GetStatus()
            });
        }
        [HttpGet("overdue-books")]
        [Authorize(Roles = "admin")] // Только для администратора
        public async Task<IActionResult> GetOverdueBooks()
        {
            // Получаем текущую дату
            var currentDate = DateTime.UtcNow;

            // Ищем книги с истекшей датой возврата
            var overdueBooks = await _context.Books
                .Include(b => b.Authors) // Подгружаем данные об авторах
                .Where(b => !b.IsAvailable && b.DueDate < currentDate)
                .ToListAsync();

            // Если таких книг нет, возвращаем соответствующее сообщение
            if (!overdueBooks.Any())
            {
                return Ok(new { message = "Нет просроченных книг." });
            }

            // Возвращаем список просроченных книг с их данными
            return Ok(overdueBooks.Select(b => new
            {
                b.Id,
                b.Title,
                b.RentedByUserId,
                b.DateRented,
                b.DueDate,
                Authors = b.Authors.Select(a => a.Name).ToList()
            }));
        }
        [HttpPost("{bookId}/return")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ReturnBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound(new { message = "Книга не найдена." });

            if (book.IsAvailable)
                return BadRequest(new { message = "Книга уже доступна." });

            book.ReturnBook();
            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Книга '{book.Title}' успешно возвращена." });
        }

        [HttpGet("most-rented-author")]
        [Authorize] // Запрос доступен только авторизованным пользователям
        public async Task<IActionResult> GetMostRentedAuthor()
        {
            var mostRentedAuthor = await _context.Authors
                .Select(a => new
                {
                    Author = a.Name,
                    RentedBookCount = a.Books.Count(b => !b.IsAvailable) // Считаем только арендованные книги
                })
                .OrderByDescending(a => a.RentedBookCount) // Сортируем по убыванию количества арендованных книг
                .FirstOrDefaultAsync(); // Берём автора с максимальным количеством арендованных книг

            if (mostRentedAuthor == null || mostRentedAuthor.RentedBookCount == 0)
            {
                return NotFound(new { message = "Нет авторов с арендованными книгами." });
            }

            return Ok(new
            {
                mostRentedAuthor.Author,
                mostRentedAuthor.RentedBookCount
            });
        }

    }
}
