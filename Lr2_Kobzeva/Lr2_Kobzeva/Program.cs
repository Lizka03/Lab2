using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lr2_Kobzeva.Models; // Namespace ������ User � ApplicationDbContext

var builder = WebApplication.CreateBuilder(args);

// ��������� �������� ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddScoped<Manager>();
// ����������� JWT-��������������
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // �������� �������� ������
            ValidateAudience = true, // �������� ��������� ������
            ValidateLifetime = true, // �������� ����� �������� ������
            ValidateIssuerSigningKey = true, // �������� ������� ������
            ValidIssuer = "EM", // ���������� ������������� �������
            ValidAudience = "APIclients", // ��������� ������
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("superSecretKeyMustBeLoooooong32bitsMore")) // ��������� ����
        };
    });

// ��������� �����������
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // ���������� ������ �������� �� ������
    });


var app = builder.Build();

// Middleware
app.UseHttpsRedirection();

app.UseRouting();

// ���������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
