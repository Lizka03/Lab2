using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lr2_Kobzeva.Models; // Namespace модели User и ApplicationDbContext

var builder = WebApplication.CreateBuilder(args);

// Добавляем контекст базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настраиваем JWT-аутентификацию
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Проверка издателя токена
            ValidateAudience = true, // Проверка аудитории токена
            ValidateLifetime = true, // Проверка срока действия токена
            ValidateIssuerSigningKey = true, // Проверка подписи токена
            ValidIssuer = "PT", // Уникальный идентификатор сервиса
            ValidAudience = "APIclients", // Аудитория токена
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("superSecretKeyMustBeLoooooong32bitsMore")) // Секретный ключ
        };
    });

// Добавляем контроллеры
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();

app.UseRouting();

// Используем аутентификацию и авторизацию
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
