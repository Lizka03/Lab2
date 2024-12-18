namespace Lr2_Kobzeva.Models
{
    public class User
    {
        // Свойства (данные пользователя)
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool is_admin { get; set; }
        public List<int> RentedBookIds { get; set; } = new List<int>();

        // Бизнес-логика
        public void AddRentedBook(int bookId)
        {
            if (!RentedBookIds.Contains(bookId))
                RentedBookIds.Add(bookId);
        }

        public void ReturnBook(int bookId)
        {
            if (RentedBookIds.Contains(bookId))
                RentedBookIds.Remove(bookId);
        }

        public int GetRentedBookCount()
        {
            return RentedBookIds.Count;
        }
    }

}
