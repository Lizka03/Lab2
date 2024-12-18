namespace Lr2_Kobzeva.Models
{
    public class Author
    {
        public int Id { get; set; } // Уникальный идентификатор автора
        public string Name { get; set; } // Имя автора

        // Навигационное свойство для связи с книгами
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
