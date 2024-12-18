using Lr2_Kobzeva.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Добавляем DbSet для моделей
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; } // Новый DbSet для авторов

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настраиваем связь многие-ко-многим между Book и Author
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(j => j.ToTable("BookAuthors")); // Таблица связывания
    }
}
