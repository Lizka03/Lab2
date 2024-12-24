namespace Lr2_Kobzeva.Models
{
    public class Book
    {
        // Свойства (данные о книге)
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? RentedByUserId { get; set; }
        public DateTime? DateRented { get; set; } // Дата аренды
        public DateTime? DueDate { get; set; } // Дата возврата

        // Связь с авторами
        public List<Author> Authors { get; set; } = new List<Author>();
        // Бизнес-логика
        public void RentBook(int userId)
        {
            if (IsAvailable)
            {
                IsAvailable = false;
                RentedByUserId = userId;
                DateRented = DateTime.Now; // Фиксируем дату взятия книги
                DueDate = DateRented.Value.AddMonths(1); // Добавляем фиксированный срок (1 месяц)
            }
        }

        public void ReturnBook()
        {
            if (!IsAvailable)
            {
                IsAvailable = true;
                RentedByUserId = null;
                DateRented = null; // Сбрасываем дату аренды
                DueDate = null; // Сбрасываем дату возврата
            }
        }

        public string GetStatus()
        {
            if (IsAvailable)
                return "Доступна";
            var overdue = DateTime.Now > DueDate ? " (просрочена)" : "";
            return $"Арендована пользователем {RentedByUserId}, вернуть до {DueDate:yyyy-MM-dd}{overdue}";
        }
    }

}
