using System.Security.Cryptography;
using System.Text;

namespace Lr2_Kobzeva.Models
{
    public class User
    {
        // Свойства (данные пользователя)
        public int Id { get; set; }
        public string Username { get; set; }
        private byte[] password; // Поле для хранения пароля в виде байтов
        public string Email { get; set; }
        public List<int> RentedBookIds { get; set; } = new List<int>();
        public string Login { get; set; } // Логин для входа
        // Хэшированный пароль через MD5
        public string Password
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var b in MD5.Create().ComputeHash(password))
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            set { password = Encoding.UTF8.GetBytes(value); }
        }

        // Роль администратора
        public bool IsAdmin { get; set; }

        // Проверка правильности пароля
        public bool CheckPassword(string inputPassword) => inputPassword == Password;
        // Бизнес-логика
        public void AddRentedBook(int bookId) //Добавляет книгу в список
        {
            if (!RentedBookIds.Contains(bookId))
                RentedBookIds.Add(bookId);
        }

        public void ReturnBook(int bookId) //Удаляет книгу из списка
        {
            if (RentedBookIds.Contains(bookId))
                RentedBookIds.Remove(bookId);
        }

        public int GetRentedBookCount()//Возвращает общее количество
        {
            return RentedBookIds.Count;
        }
    }

}
