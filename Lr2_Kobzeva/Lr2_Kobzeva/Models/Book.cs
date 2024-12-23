namespace Lr2_Kobzeva.Models
{
    public class Book
    {
        // Свойства (данные о книге)
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? RentedByUserId { get; set; }

        // Связь с авторами
        public List<Author> Authors { get; set; } = new List<Author>();
        // Бизнес-логика
        public void RentBook(int userId)
        {
            if (IsAvailable)
            {
                IsAvailable = false;
                RentedByUserId = userId;
            }
        }

        public void ReturnBook()
        {
            if (!IsAvailable)
            {
                IsAvailable = true;
                RentedByUserId = null;
            }
        }

        public string GetStatus()
        {
            return IsAvailable ? "Доступна" : $"Арендована пользователем {RentedByUserId}";
        }
    }

}
